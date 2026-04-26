using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfNumericUpDown;

namespace SimpleWpfPropertyGrid;

public partial class PropertyGrid : UserControl
{
    public static readonly DependencyProperty TargetObjectProperty =
        DependencyProperty.Register(
            nameof(TargetObject), typeof(object), typeof(PropertyGrid),
            new PropertyMetadata(null, OnTargetObjectChanged));

    public object? TargetObject
    {
        get => GetValue(TargetObjectProperty);
        set => SetValue(TargetObjectProperty, value);
    }

    private static void OnTargetObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((PropertyGrid)d).RebuildUI();

    public PropertyGrid()
    {
        InitializeComponent();
    }

    private void RebuildUI()
    {
        PropertiesGrid.RowDefinitions.Clear();
        PropertiesGrid.Children.Clear();

        if (TargetObject == null) return;

        var props = TargetObject.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .OrderBy(p => p.Name)
            .ToList();

        for (int i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            PropertiesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var background = i % 2 == 0
                ? new SolidColorBrush(Color.FromRgb(248, 249, 250))
                : Brushes.White;

            var nameBorder = new Border
            {
                Background = background,
                Padding = new Thickness(8, 7, 8, 7),
                BorderBrush = new SolidColorBrush(Color.FromRgb(222, 226, 230)),
                BorderThickness = new Thickness(0, 0, 1, 1)
            };
            nameBorder.Child = new TextBlock
            {
                Text = GetPropertyLabel(prop),
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = prop.Name
            };
            Grid.SetRow(nameBorder, i);
            Grid.SetColumn(nameBorder, 0);
            PropertiesGrid.Children.Add(nameBorder);

            var valueBorder = new Border
            {
                Background = background,
                Padding = new Thickness(6, 4, 6, 4),
                BorderBrush = new SolidColorBrush(Color.FromRgb(222, 226, 230)),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            valueBorder.Child = prop.CanWrite ? CreateEditor(prop) : CreateReadOnlyDisplay(prop);
            Grid.SetRow(valueBorder, i);
            Grid.SetColumn(valueBorder, 1);
            PropertiesGrid.Children.Add(valueBorder);
        }
    }

    internal static string FormatPropertyName(string name)
    {
        var sb = new StringBuilder(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }

    private static string GetPropertyLabel(PropertyInfo prop) =>
        prop.GetCustomAttribute<PropertyGridLabelAttribute>()?.Label
        ?? FormatPropertyName(prop.Name);

    private UIElement CreateReadOnlyDisplay(PropertyInfo prop) =>
        new TextBlock
        {
            Text = prop.GetValue(TargetObject)?.ToString() ?? "(null)",
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.Gray,
            FontStyle = FontStyles.Italic
        };

    private UIElement CreateEditor(PropertyInfo prop)
    {
        var type = prop.PropertyType;
        var value = prop.GetValue(TargetObject);

        if (type == typeof(string))
            return CreateStringEditor(prop, (string?)value);

        if (type == typeof(bool))
            return CreateBoolEditor(prop, value is bool b && b);

        if (type == typeof(DateTime))
            return CreateDateTimeEditor(prop, value as DateTime?);

        if (type.IsEnum)
            return CreateEnumEditor(prop, value);

        if (IsNumericType(type))
            return CreateNumericEditor(prop, value);

        // Any reference type (class) gets an "Open" button for recursive editing
        if (!type.IsValueType)
            return CreateObjectButton(prop, value);

        return CreateReadOnlyDisplay(prop);
    }

    private UIElement CreateStringEditor(PropertyInfo prop, string? value)
    {
        var tb = new TextBox
        {
            Text = value ?? string.Empty,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(4, 3, 4, 3)
        };
        tb.LostFocus += (_, _) =>
        {
            try { prop.SetValue(TargetObject, tb.Text); }
            catch { /* ignore */ }
        };
        return tb;
    }

    private static UIElement CreateBoolEditor(PropertyInfo prop, bool value)
    {
        var cb = new CheckBox
        {
            IsChecked = value,
            VerticalAlignment = VerticalAlignment.Center
        };
        // Capture prop in closure; TargetObject captured via the outer instance field reference
        cb.Checked += (s, _) =>
        {
            var pg = FindPropertyGrid((CheckBox)s!);
            try { prop.SetValue(pg?.TargetObject, true); }
            catch { /* ignore */ }
        };
        cb.Unchecked += (s, _) =>
        {
            var pg = FindPropertyGrid((CheckBox)s!);
            try { prop.SetValue(pg?.TargetObject, false); }
            catch { /* ignore */ }
        };
        return cb;
    }

    private static UIElement CreateDateTimeEditor(PropertyInfo prop, DateTime? value)
    {
        var dp = new DatePicker
        {
            SelectedDate = value,
            VerticalAlignment = VerticalAlignment.Center
        };
        dp.SelectedDateChanged += (s, _) =>
        {
            var pg = FindPropertyGrid((DatePicker)s!);
            try
            {
                if (dp.SelectedDate.HasValue)
                    prop.SetValue(pg?.TargetObject, dp.SelectedDate.Value);
            }
            catch { /* ignore */ }
        };
        return dp;
    }

    private static UIElement CreateEnumEditor(PropertyInfo prop, object? value)
    {
        var cb = new ComboBox
        {
            ItemsSource = Enum.GetValues(prop.PropertyType),
            SelectedItem = value,
            VerticalAlignment = VerticalAlignment.Center
        };
        cb.SelectionChanged += (s, _) =>
        {
            var pg = FindPropertyGrid((ComboBox)s!);
            try { prop.SetValue(pg?.TargetObject, cb.SelectedItem); }
            catch { /* ignore */ }
        };
        return cb;
    }

    private static UIElement CreateNumericEditor(PropertyInfo prop, object? value)
    {
        if (prop.GetCustomAttribute<PropertyGridNumericUpDownAttribute>() != null)
            return CreateNumericUpDownEditor(prop, value);

        var tb = new TextBox
        {
            Text = value?.ToString() ?? "0",
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(4, 3, 4, 3)
        };
        tb.LostFocus += (s, _) =>
        {
            var pg = FindPropertyGrid((TextBox)s!);
            try
            {
                var converted = Convert.ChangeType(tb.Text, prop.PropertyType);
                prop.SetValue(pg?.TargetObject, converted);
            }
            catch
            {
                tb.Text = prop.GetValue(pg?.TargetObject)?.ToString() ?? "0";
            }
        };
        return tb;
    }

    private static UIElement CreateNumericUpDownEditor(PropertyInfo prop, object? value)
    {
        var step = prop.GetCustomAttribute<PropertyGridStepAttribute>()?.Step ?? 1.0;
        var decimals = prop.GetCustomAttribute<PropertyGridDecimalsAttribute>()?.Decimals
                       ?? DefaultDecimalPlaces(prop.PropertyType);

        var nud = new NumericUpDown
        {
            Value = Convert.ToDouble(value ?? 0),
            Step = step,
            DecimalPlaces = decimals,
            VerticalAlignment = VerticalAlignment.Center
        };
        nud.ValueChanged += (s, e) =>
        {
            var pg = FindPropertyGrid((NumericUpDown)s!);
            try
            {
                var converted = Convert.ChangeType(e.NewValue, prop.PropertyType);
                prop.SetValue(pg?.TargetObject, converted);
            }
            catch { /* ignore out-of-range conversions */ }
        };
        return nud;
    }

    private static int DefaultDecimalPlaces(Type type) =>
        type == typeof(double) || type == typeof(float) || type == typeof(decimal) ? 2 : 0;

    private UIElement CreateObjectButton(PropertyInfo prop, object? value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        panel.Children.Add(new TextBlock
        {
            Text = $"({prop.PropertyType.Name})",
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            Margin = new Thickness(0, 0, 8, 0),
            FontStyle = FontStyles.Italic,
            FontSize = 11
        });

        var btn = new Button
        {
            Content = "Open",
            Padding = new Thickness(14, 3, 14, 3),
            MinWidth = 64
        };
        btn.Click += (_, _) =>
        {
            var obj = prop.GetValue(TargetObject);
            if (obj == null)
            {
                try
                {
                    obj = Activator.CreateInstance(prop.PropertyType);
                    prop.SetValue(TargetObject, obj);
                }
                catch { return; }
            }
            var window = new PropertyGridWindow(obj!, GetPropertyLabel(prop))
            {
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        };
        panel.Children.Add(btn);

        return panel;
    }

    // Walk up the visual tree to find the owning PropertyGrid, needed by static event handlers.
    private static PropertyGrid? FindPropertyGrid(DependencyObject child)
    {
        var current = System.Windows.Media.VisualTreeHelper.GetParent(child);
        while (current != null)
        {
            if (current is PropertyGrid pg) return pg;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static bool IsNumericType(Type type) =>
        type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
        type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) ||
        type == typeof(double) || type == typeof(float) || type == typeof(decimal);
}
