using System.Windows;
using System.Windows.Controls;

namespace insoles.Behaviour
{
    public static class IndividualSelectionBehaviour
    {
        public static readonly DependencyProperty PropagateSelectionProperty =
            DependencyProperty.RegisterAttached("PropagateSelection", typeof(bool), typeof(IndividualSelectionBehaviour), new PropertyMetadata(false, OnPropagateSelectionChanged));

        public static bool GetPropagateSelection(DependencyObject obj)
        {
            return (bool)obj.GetValue(PropagateSelectionProperty);
        }

        public static void SetPropagateSelection(DependencyObject obj, bool value)
        {
            obj.SetValue(PropagateSelectionProperty, value);
        }

        private static void OnPropagateSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem treeViewItem = d as TreeViewItem;
            if (treeViewItem == null)
                return;

            bool propagateSelection = (bool)e.NewValue;

            if (propagateSelection)
                treeViewItem.Selected += TreeViewItem_Selected;
            else
                treeViewItem.Selected -= TreeViewItem_Selected;
        }

        private static void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = e.OriginalSource as TreeViewItem;
            if (treeViewItem != null)
                treeViewItem.IsSelected = true;
        }
    }

}
