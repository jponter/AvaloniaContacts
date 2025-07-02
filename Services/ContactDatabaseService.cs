using System;
using System.Collections.ObjectModel;
using System.IO;
using AvaloniaContacts.Models;
using Microsoft.Data.Sqlite;

namespace AvaloniaContacts.Services;

public static class ContactDatabaseService
{
    static string  folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    static string  dbPath = Path.Combine(folder, "contacts.db");
    static string connectionString = $"Data Source={dbPath}";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Contacts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT,
                LastName TEXT,
                Email TEXT,
                Phone TEXT,
                Address TEXT,
                Details TEXT
            );
        """;
        tableCmd.ExecuteNonQuery();
    }

    public static void SaveContacts(ObservableCollection<Contact> contacts)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Contacts;";
        deleteCmd.ExecuteNonQuery();

        foreach (var c in contacts)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = """
                INSERT INTO Contacts (FirstName, LastName, Email, Phone, Address, Details)
                VALUES ($first, $last, $email, $phone, $address, $details);
            """;
            insertCmd.Parameters.AddWithValue("$first", c.FirstName);
            insertCmd.Parameters.AddWithValue("$last", c.LastName);
            insertCmd.Parameters.AddWithValue("$email", c.Email);
            insertCmd.Parameters.AddWithValue("$phone", c.Phone);
            insertCmd.Parameters.AddWithValue("$address", c.Address);
            insertCmd.Parameters.AddWithValue("$details", c.Details);
            insertCmd.ExecuteNonQuery();
        }
    }

    public static ObservableCollection<Contact> LoadContacts()
    {
        var result = new ObservableCollection<Contact>();
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT FirstName, LastName, Email, Phone, Address, Details FROM Contacts;";
        using var reader = selectCmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new Contact(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5)
            ));
        }

        return result;
    }
}
