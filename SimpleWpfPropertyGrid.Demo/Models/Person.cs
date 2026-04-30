using System;
using System.Collections.Generic;
using SimpleWpfPropertyGrid;

namespace SimpleWpfPropertyGrid.Demo.Models;

public class Person
{
    [PropertyGridLabel("Full Name")]
    public string Name { get; set; } = string.Empty;

    [PropertyGridLabel("Age (years)")]
    [PropertyGridNumericUpDown]
    public int Age { get; set; }

    [PropertyGridLabel("Date of Birth")]
    public DateTime BirthDate { get; set; }

    public bool IsEmployed { get; set; }

    [PropertyGridLabel("Employment Status")]
    public EmploymentStatus Status { get; set; }

    [PropertyGridLabel("Home Address")]
    public Address HomeAddress { get; set; } = new();

    [PropertyGridLabel("Work Address")]
    public Address WorkAddress { get; set; } = new();

    [PropertyGridLabel("Contact Details")]
    public ContactInfo Contact { get; set; } = new();

    [PropertyGridLabel("Skills")]
    public List<string> Skills { get; set; } = new();

    [PropertyGridLabel("Previous Addresses")]
    public List<Address> PreviousAddresses { get; set; } = new();
}
