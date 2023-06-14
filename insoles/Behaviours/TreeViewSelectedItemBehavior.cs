using System.Windows;
using System.Windows.Controls;

namespace insoles.Behaviour
{
    public static class TreeViewSelectedItemBehavior
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItem",
                typeof(object),
                typeof(TreeViewSelectedItemBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static object GetSelectedItem(DependencyObject obj)
        {
            return (object)obj.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var treeViewItem = e.NewValue as TreeViewItem;
            if (treeViewItem != null)
            {
                var viewModel = treeViewItem.DataContext;
                var selectedItemProperty = viewModel.GetType().GetProperty("SelectedItem");
                selectedItemProperty.SetValue(viewModel, treeViewItem.DataContext);
            }
        }
    }

}
