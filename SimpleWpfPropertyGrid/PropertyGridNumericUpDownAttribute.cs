using System;

namespace SimpleWpfPropertyGrid;

/// <summary>
/// Renders a numeric property using a <see cref="WpfNumericUpDown.NumericUpDown"/> spinner
/// instead of a plain TextBox. Combine with <see cref="PropertyGridStepAttribute"/> and
/// <see cref="PropertyGridDecimalsAttribute"/> to control behaviour.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PropertyGridNumericUpDownAttribute : Attribute { }
