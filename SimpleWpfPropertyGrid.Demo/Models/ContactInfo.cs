using System;

namespace SimpleWpfPropertyGrid.Demo.Models;

public class ContactInfo
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime LastContacted { get; set; } = DateTime.Today;
}
