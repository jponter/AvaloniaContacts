using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaContacts.Models;
using AvaloniaContacts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaContacts.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    public MainWindowViewModel()
    {
            ContactDatabaseService.Initialize();
            Contacts = ContactDatabaseService.LoadContacts();

            Contacts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(FilteredContacts));
            
            // 🔗 listen for adds/removes
            Contacts.CollectionChanged += Contacts_CollectionChanged;

            // 🔗 listen for property changes on existing items
            foreach (var c in Contacts)
                c.PropertyChanged += Contact_PropertyChanged;

    }
    
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _saveTokens = new();
    private Contact? _previousSelectedContact;
    private ObservableCollection<Contact> Contacts { get; set; }
    
    [ObservableProperty]
    private Contact? _selectedContact;
    
    //search box text
    [ObservableProperty] private string? searchText = "";

    partial void OnSearchTextChanged(string? _) =>
        OnPropertyChanged(nameof(FilteredContacts));
    
    //the list the UI actually shows
    public IEnumerable<Contact> FilteredContacts =>
        string.IsNullOrWhiteSpace(SearchText)
            ? Contacts
            : Contacts.Where(c => c.FullName.Contains(SearchText!, StringComparison.OrdinalIgnoreCase));

    // [ObservableProperty] private bool isLocked = true;
    partial void OnSelectedContactChanged(Contact? value)
    {
        // lock the one we’re leaving
        if (_previousSelectedContact is not null)
            _previousSelectedContact.IsLocked = true;

        _previousSelectedContact = value;
        // the newly selected contact shows its own IsLocked (likely true)
    }

    [RelayCommand]
    private void AddContact()
    {
        var newContact = new Contact("New", "Contact", "", "", "", "")
        {
            IsLocked = false,
        };
        Contacts.Add(newContact);
        SelectedContact = newContact;
    }
    
    private void Contact_PropertyChanged(object? s, PropertyChangedEventArgs e)
    {
        if (s is not Contact c || e.PropertyName == nameof(Contact.Id))
            return;

        // Cancel any pending write for this contact
        if (_saveTokens.TryGetValue(c.Id, out var oldCts))
        {
            oldCts.Cancel();
            oldCts.Dispose();
        }

        // Schedule a new one
        var cts = new CancellationTokenSource();
        _saveTokens[c.Id] = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500, cts.Token);          // 500 ms debounce window
                ContactDatabaseService.UpdateContact(c);   // after the quiet period
            }
            catch (TaskCanceledException) { /* ignored */ }
            finally
            {
                _saveTokens.TryRemove(c.Id, out _);
                cts.Dispose();
            }
        }, cts.Token);
    }

    

    
    private void Contacts_CollectionChanged(object? s, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (Contact c in e.NewItems)
            {
                // insert into DB, get generated Id
                c.Id = ContactDatabaseService.InsertContact(c);

                // start listening to property changes
                c.PropertyChanged += Contact_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (Contact c in e.OldItems)
            {
                c.PropertyChanged -= Contact_PropertyChanged;
                ContactDatabaseService.DeleteContact(c.Id);
            }
        }
        
        // Clean up any pending tokens for removed items
        if (e.OldItems != null)
            foreach (Contact old in e.OldItems)
                if (_saveTokens.TryRemove(old.Id, out var t)) t.Cancel();
    }

 

    public void RemoveSelectedContact()
    {
        if (SelectedContact != null)
            Contacts.Remove(SelectedContact);
    }

    private bool CanRemoveContact() => SelectedContact != null;



 
   
    [RelayCommand]
    private void SaveToDatabase()
    {
        ContactDatabaseService.SaveContacts(Contacts);
    }


}

