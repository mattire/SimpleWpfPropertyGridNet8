using System;

namespace SimpleWpfPropertyGrid;

/// <summary>
/// Sets the increment/decrement step of a <see cref="PropertyGridNumericUpDownAttribute"/>
/// spinner. Defaults to 1 when not specified.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PropertyGridStepAttribute : Attribute
{
    public double Step { get; }

    public PropertyGridStepAttribute(double step)
    {
        if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step), "Step must be positive.");
        Step = step;
    }
}
