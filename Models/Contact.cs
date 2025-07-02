using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaContacts.Models;

public partial class Contact : ObservableObject
{
    public Contact(string firstname, string lastname, string email, string phone, string address, string details)
    {
        FirstName = firstname;
        LastName = lastname;
        Email = email;
        Phone = phone;
        Address = address;
        Details = details;
    }

    public string FullName => $"{FirstName} {LastName}";
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FullName))] private string _firstName;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FullName))] private string _lastName;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _phone;
    [ObservableProperty] private string _address;
    [ObservableProperty] private string _details;
    


   


  

}