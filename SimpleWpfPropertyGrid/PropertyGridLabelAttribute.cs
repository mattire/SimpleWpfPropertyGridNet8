using System;

namespace SimpleWpfPropertyGrid;

/// <summary>
/// Overrides the label shown for a property in <see cref="PropertyGrid"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PropertyGridLabelAttribute : Attribute
{
    public string Label { get; }

    public PropertyGridLabelAttribute(string label)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
    }
}
