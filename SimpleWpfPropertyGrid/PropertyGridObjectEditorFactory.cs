using System.Windows;
using WpfCollectionEditor;

namespace SimpleWpfPropertyGrid;

internal sealed class PropertyGridObjectEditorFactory : IObjectEditorFactory
{
    private readonly Window? _defaultOwner;

    public PropertyGridObjectEditorFactory(Window? defaultOwner) => _defaultOwner = defaultOwner;

    public void EditObject(object target, string title, Window? owner)
    {
        new PropertyGridWindow(target, title) { Owner = owner ?? _defaultOwner }.ShowDialog();
    }
}
