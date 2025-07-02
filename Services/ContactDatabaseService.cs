using System;
using System.Collections.ObjectModel;
using System.IO;
using AvaloniaContacts.Models;
using Microsoft.Data.Sqlite;

namespace AvaloniaContacts.Services;

public static class ContactDatabaseService
{
    static string  folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string appName = "AvaloniaContacts";
    private static readonly string dbName = "contacts.db";
    
    private static readonly string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName, dbName);
    // static string  dbPath = Path.Combine(folder, "contacts.db");
    static string connectionString = $"Data Source={dbPath}";

    public static void Initialize()
    {
        var directoryPath = Path.GetDirectoryName(dbPath);
        if (directoryPath != null) // Ensure directoryPath is not null
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        
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

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
                              SELECT Id, FirstName, LastName, Email, Phone, Address, Details
                              FROM Contacts;
                          """;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var contact = new Contact(
                reader.GetString(1),      // FirstName
                reader.GetString(2),      // LastName
                reader.GetString(3),      // Email
                reader.GetString(4),      // Phone
                reader.GetString(5),      // Address
                reader.GetString(6)       // Details
            )
            {
                Id = reader.GetInt32(0)   //  Id
            };

            result.Add(contact);
        }
        return result;
    }
    
    public static int InsertContact(Contact c)
    {
        using var con = new SqliteConnection(connectionString);
        con.Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = """
                              INSERT INTO Contacts (FirstName, LastName, Email, Phone, Address, Details)
                              VALUES ($f,$l,$e,$p,$a,$d);
                              SELECT last_insert_rowid();
                          """;
        cmd.Parameters.AddWithValue("$f", c.FirstName);
        cmd.Parameters.AddWithValue("$l", c.LastName);
        cmd.Parameters.AddWithValue("$e", c.Email);
        cmd.Parameters.AddWithValue("$p", c.Phone);
        cmd.Parameters.AddWithValue("$a", c.Address);
        cmd.Parameters.AddWithValue("$d", c.Details);
        return Convert.ToInt32(cmd.ExecuteScalar()!);   // returns new Id
    }

    public static void UpdateContact(Contact c)
    {
        using var con = new SqliteConnection(connectionString);
        con.Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = """
                              UPDATE Contacts
                              SET FirstName=$f, LastName=$l, Email=$e, Phone=$p, Address=$a, Details=$d
                              WHERE Id=$id;
                          """;
        cmd.Parameters.AddWithValue("$id", c.Id);
        cmd.Parameters.AddWithValue("$f",  c.FirstName);
        cmd.Parameters.AddWithValue("$l",  c.LastName);
        cmd.Parameters.AddWithValue("$e",  c.Email);
        cmd.Parameters.AddWithValue("$p",  c.Phone);
        cmd.Parameters.AddWithValue("$a",  c.Address);
        cmd.Parameters.AddWithValue("$d",  c.Details);
        cmd.ExecuteNonQuery();
    }

    public static void DeleteContact(int id)
    {
        using var con = new SqliteConnection(connectionString);
        con.Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "DELETE FROM Contacts WHERE Id=$id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}
