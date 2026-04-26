using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfNumericUpDown;

public partial class NumericUpDown : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericUpDown),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, CoerceValue));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double), typeof(NumericUpDown),
            new PropertyMetadata(1.0));

    public static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(0, OnDecimalPlacesChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumericUpDown),
            new PropertyMetadata(double.NegativeInfinity));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumericUpDown),
            new PropertyMetadata(double.PositiveInfinity));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    #endregion

    #region Routed Event

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(NumericUpDown));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    #endregion

    private bool _suppressTextUpdate;

    public NumericUpDown()
    {
        InitializeComponent();
        IsKeyboardFocusWithinChanged += OnKeyboardFocusWithinChanged;
        Loaded += (_, _) => UpdateText();
    }

    #region Property-changed callbacks

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (NumericUpDown)d;
        ctrl.UpdateText();
        ctrl.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(
            (double)e.OldValue, (double)e.NewValue, ValueChangedEvent));
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var ctrl = (NumericUpDown)d;
        return Math.Clamp((double)baseValue, ctrl.Minimum, ctrl.Maximum);
    }

    private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((NumericUpDown)d).UpdateText();

    #endregion

    private void UpdateText()
    {
        if (ValueTextBox == null) return;
        _suppressTextUpdate = true;
        ValueTextBox.Text = Value.ToString($"F{DecimalPlaces}", CultureInfo.CurrentCulture);
        _suppressTextUpdate = false;
    }

    private void CommitText()
    {
        if (_suppressTextUpdate) return;
        if (double.TryParse(ValueTextBox.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsed))
            Value = parsed; // CoerceValue clamps to [Minimum, Maximum]
        else
            UpdateText(); // restore last valid display
    }

    private void Increment() => Value = RoundToStep(Value + Step);
    private void Decrement() => Value = RoundToStep(Value - Step);

    // Round after each step to prevent floating-point drift (e.g. 0.1+0.1+0.1 ≠ 0.3).
    // Uses the larger of DecimalPlaces and the step's own decimal count as the precision.
    private double RoundToStep(double value)
    {
        int places = Math.Max(DecimalPlaces, StepDecimalCount());
        return Math.Round(value, Math.Min(places, 15));
    }

    private int StepDecimalCount()
    {
        if (Step == 0 || !double.IsFinite(Step)) return 0;
        var s = Step.ToString("R", CultureInfo.InvariantCulture);
        int dot = s.IndexOf('.');
        return dot < 0 ? 0 : s.Length - dot - 1;
    }

    #region Event handlers

    private void OnKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        OuterBorder.BorderBrush = IsKeyboardFocusWithin
            ? SystemColors.HighlightBrush
            : new SolidColorBrush(Color.FromRgb(0xAD, 0xAD, 0xAD));
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:   Increment(); e.Handled = true; break;
            case Key.Down: Decrement(); e.Handled = true; break;
        }
        base.OnPreviewKeyDown(e);
    }

    private void UpButton_Click(object sender, RoutedEventArgs e) => Increment();
    private void DownButton_Click(object sender, RoutedEventArgs e) => Decrement();

    private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e) => CommitText();

    private void ValueTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { CommitText(); e.Handled = true; }
    }

    // Select all text when the box receives keyboard focus so typing replaces the value immediately.
    private void ValueTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        => ValueTextBox.SelectAll();

    // Clicking into an unfocused TextBox normally positions the caret; intercept to focus+select-all instead.
    private void ValueTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!ValueTextBox.IsKeyboardFocusWithin)
        {
            e.Handled = true;
            ValueTextBox.Focus();
        }
    }

    #endregion
}
