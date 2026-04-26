using System.Windows;

namespace SimpleWpfPropertyGrid;

public partial class PropertyGridWindow : Window
{
    public PropertyGridWindow(object targetObject, string propertyName)
    {
        InitializeComponent();
        Title = propertyName;
        TypeInfoText.Text = $"Editing: {targetObject.GetType().FullName}";
        PropertyGridControl.TargetObject = targetObject;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
