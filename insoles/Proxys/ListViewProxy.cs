using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace insoles.Proxy
{
    public class ListViewProxy : Freezable
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(ListViewProxy));

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public ListView ListView
        {
            get { return (ListView)GetValue(ListViewProperty); }
            set { SetValue(ListViewProperty, value); }
        }

        public static readonly DependencyProperty ListViewProperty =
            DependencyProperty.Register("ListView", typeof(ListView), typeof(ListViewProxy));

        protected override Freezable CreateInstanceCore()
        {
            return new ListViewProxy();
        }
    }
}
