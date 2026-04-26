using System;

namespace SimpleWpfPropertyGrid;

/// <summary>
/// Sets the number of decimal places displayed by a <see cref="PropertyGridNumericUpDownAttribute"/>
/// spinner. Defaults to 0 for integer types and 2 for floating-point types when not specified.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PropertyGridDecimalsAttribute : Attribute
{
    public int Decimals { get; }

    public PropertyGridDecimalsAttribute(int decimals)
    {
        if (decimals < 0) throw new ArgumentOutOfRangeException(nameof(decimals), "Decimals must be >= 0.");
        Decimals = decimals;
    }
}
