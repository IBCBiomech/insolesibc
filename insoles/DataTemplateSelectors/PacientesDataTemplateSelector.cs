using insoles.Model;
using insoles.Models;
using Syncfusion.UI.Xaml.TreeView.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace insoles.DataTemplateSelectors
{
    public class PacientesDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PacientesTemplate { get; set; }
        public DataTemplate PacienteTemplate { get; set; }
        public DataTemplate TestsTemplate { get; set; }
        public DataTemplate InformesTemplate { get; set; }
        public DataTemplate TestTemplate { get; set; }
        public DataTemplate InformeTemplate { get; set; }
        public DataTemplate TextTestFileTemplate { get; set; }
        public DataTemplate VideoTestFileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is TreeViewNode)
            {
                var node = (TreeViewNode)item;
                if (node.Content is PacientesTreeView)
                {
                    return PacientesTemplate;
                }
                else if (node.Content is PacienteTreeView)
                {
                    return PacienteTemplate;
                }
                else if (node.Content is TestsTreeView)
                {
                    return TestsTemplate;
                }
                else if (node.Content is InformesTreeView)
                {
                    return InformesTemplate;
                }
                else if (node.Content is TestTreeView)
                {
                    return TestTemplate;
                }
                else if (node.Content is InformeTreeView)
                {
                    return InformeTemplate;
                }
                else if(node.Content is TextTestFileTreeView)
                {
                    return TextTestFileTemplate;
                }
                else if (node.Content is VideoTestFileTreeView)
                {
                    return VideoTestFileTemplate;
                }
            }

            // Return a default template if the item type is not recognized
            return base.SelectTemplate(item, container);
        }
    }
}
