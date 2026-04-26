using System;
using System.Text;
using System.Windows;
using SimpleWpfPropertyGrid.Demo.Models;

namespace SimpleWpfPropertyGrid.Demo;

public partial class MainWindow : Window
{
    private Person _person = CreateDefaultPerson();

    public MainWindow()
    {
        InitializeComponent();
        SubtitleText.Text = $"Editing: {typeof(Person).FullName}";
        MainPropertyGrid.TargetObject = _person;
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _person = CreateDefaultPerson();
        MainPropertyGrid.TargetObject = _person;
    }

    private void ShowSummary_Click(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Name:        {_person.Name}");
        sb.AppendLine($"Age:         {_person.Age}");
        sb.AppendLine($"Birth date:  {_person.BirthDate:yyyy-MM-dd}");
        sb.AppendLine($"Employed:    {_person.IsEmployed}");
        sb.AppendLine($"Status:      {_person.Status}");
        sb.AppendLine();
        sb.AppendLine("Home Address:");
        AppendAddress(sb, _person.HomeAddress);
        sb.AppendLine();
        sb.AppendLine("Work Address:");
        AppendAddress(sb, _person.WorkAddress);
        sb.AppendLine();
        sb.AppendLine("Contact:");
        sb.AppendLine($"  Email:          {_person.Contact.Email}");
        sb.AppendLine($"  Phone:          {_person.Contact.Phone}");
        sb.AppendLine($"  Last contacted: {_person.Contact.LastContacted:yyyy-MM-dd}");

        MessageBox.Show(sb.ToString(), "Person Summary", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void AppendAddress(StringBuilder sb, Address a)
    {
        sb.AppendLine($"  Street:    {a.Street}");
        sb.AppendLine($"  City:      {a.City}");
        sb.AppendLine($"  State:     {a.State}");
        sb.AppendLine($"  Zip:       {a.ZipCode}");
        sb.AppendLine($"  Lat/Lon:   {a.Location.Latitude} / {a.Location.Longitude}");
        sb.AppendLine($"  Altitude:  {a.Location.AltitudeMeters} m");
    }

    private static Person CreateDefaultPerson() => new()
    {
        Name = "Jane Smith",
        Age = 34,
        BirthDate = new DateTime(1990, 6, 15),
        IsEmployed = true,
        Status = EmploymentStatus.FullTime,
        HomeAddress = new Address
        {
            Street = "42 Elm Street",
            City = "Springfield",
            State = "IL",
            ZipCode = "62701",
            Location = new GeoLocation { Latitude = 39.7817, Longitude = -89.6501 }
        },
        WorkAddress = new Address
        {
            Street = "1 Corporate Plaza",
            City = "Chicago",
            State = "IL",
            ZipCode = "60601",
            Location = new GeoLocation { Latitude = 41.8781, Longitude = -87.6298 }
        },
        Contact = new ContactInfo
        {
            Email = "jane.smith@example.com",
            Phone = "555-0147",
            LastContacted = DateTime.Today.AddDays(-3)
        }
    };
}
