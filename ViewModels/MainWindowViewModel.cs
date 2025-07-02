using System.Collections.Generic;
using System.Collections.ObjectModel;
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


        
    }

    public ObservableCollection<Contact> Contacts { get; }
    
    [ObservableProperty]
    private Contact? _selectedContact;

    [ObservableProperty] private bool isLocked = true;

    [RelayCommand]
    private void AddContact()
    {
        var newContact = new Contact("New", "Contact", "", "", "", "");
        Contacts.Add(newContact);
        SelectedContact = newContact;
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

