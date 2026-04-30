using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using WpfNumericUpDown;

namespace WpfCollectionEditor;

public partial class CollectionEditorWindow : Window
{
    private readonly IList _list;
    private readonly Type _elementType;
    private readonly IObjectEditorFactory? _objectEditorFactory;

    public CollectionEditorWindow(IList list, Type elementType, string title, IObjectEditorFactory? objectEditorFactory = null)
    {
        InitializeComponent();
        _list = list;
        _elementType = elementType;
        _objectEditorFactory = objectEditorFactory;
        Title = title;
        BannerText.Text = $"List<{elementType.Name}>";
        BuildAddControls();
        RefreshListBox();
    }

    private void RefreshListBox()
    {
        ItemsListBox.Items.Clear();
        for (int i = 0; i < _list.Count; i++)
        {
            var item = _list[i];
            if (IsObjectType(_elementType))
            {
                var li = new ListBoxItem { Tag = i };
                var btn = new Button
                {
                    Content = $"Open {item}",
                    Tag = i,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(8, 3, 8, 3)
                };
                btn.Click += ObjectItemButton_Click;
                li.Content = btn;
                ItemsListBox.Items.Add(li);
            }
            else
            {
                ItemsListBox.Items.Add(new ListBoxItem { Content = item?.ToString() ?? "(null)", Tag = i });
            }
        }
    }

    private void ObjectItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int index || index >= _list.Count)
            return;

        foreach (ListBoxItem li in ItemsListBox.Items)
        {
            if (li.Tag is int t && t == index)
            {
                li.IsSelected = true;
                break;
            }
        }

        var item = _list[index];
        if (item != null)
            _objectEditorFactory?.EditObject(item, $"Item {index + 1}", this);
        RefreshListBox();
    }

    private void BuildAddControls()
    {
        AddControlsPanel.Children.Clear();

        if (_elementType == typeof(string))
        {
            var tb = new TextBox
            {
                Width = 200,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(4, 3, 4, 3),
                Margin = new Thickness(0, 0, 8, 0)
            };
            var addBtn = new Button { Content = "Add" };
            addBtn.Click += (_, _) =>
            {
                _list.Add(tb.Text);
                tb.Clear();
                RefreshListBox();
            };
            AddControlsPanel.Children.Add(new TextBlock { Text = "Value:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });
            AddControlsPanel.Children.Add(tb);
            AddControlsPanel.Children.Add(addBtn);
        }
        else if (_elementType == typeof(int) || _elementType == typeof(double) || _elementType == typeof(float))
        {
            int decimals = _elementType == typeof(int) ? 0 : 2;
            var nud = new NumericUpDown
            {
                Width = 120,
                DecimalPlaces = decimals,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            var addBtn = new Button { Content = "Add" };
            addBtn.Click += (_, _) =>
            {
                _list.Add(Convert.ChangeType(nud.Value, _elementType));
                nud.Value = 0;
                RefreshListBox();
            };
            AddControlsPanel.Children.Add(new TextBlock { Text = "Value:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });
            AddControlsPanel.Children.Add(nud);
            AddControlsPanel.Children.Add(addBtn);
        }
        else if (_elementType == typeof(DateTime))
        {
            var dp = new DatePicker
            {
                SelectedDate = DateTime.Today,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            var addBtn = new Button { Content = "Add" };
            addBtn.Click += (_, _) =>
            {
                if (dp.SelectedDate.HasValue)
                {
                    _list.Add(dp.SelectedDate.Value);
                    RefreshListBox();
                }
            };
            AddControlsPanel.Children.Add(new TextBlock { Text = "Date:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });
            AddControlsPanel.Children.Add(dp);
            AddControlsPanel.Children.Add(addBtn);
        }
        else if (IsObjectType(_elementType))
        {
            var addBtn = new Button { Content = $"Add New {_elementType.Name}" };
            addBtn.Click += (_, _) =>
            {
                try
                {
                    var newItem = Activator.CreateInstance(_elementType);
                    if (newItem == null) return;
                    _list.Add(newItem);
                    RefreshListBox();
                    _objectEditorFactory?.EditObject(newItem, $"New {_elementType.Name}", this);
                    RefreshListBox();
                }
                catch { }
            };
            AddControlsPanel.Children.Add(addBtn);
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsListBox.SelectedItem is ListBoxItem li && li.Tag is int index && index < _list.Count)
        {
            _list.RemoveAt(index);
            RefreshListBox();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private static bool IsObjectType(Type type) => !type.IsValueType && type != typeof(string);
}
