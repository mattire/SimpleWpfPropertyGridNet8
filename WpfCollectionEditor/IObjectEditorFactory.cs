using System.Windows;

namespace WpfCollectionEditor;

public interface IObjectEditorFactory
{
    void EditObject(object target, string title, Window? owner);
}
