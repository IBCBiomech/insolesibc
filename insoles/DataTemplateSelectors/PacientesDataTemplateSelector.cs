using insoles.Model;
using System;
using System.Collections.Generic;
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

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PacientesTreeView)
            {
                return PacientesTemplate;
            }
            else if (item is PacienteTreeView)
            {
                return PacienteTemplate;
            }
            else if (item is TestsTreeView)
            {
                return TestsTemplate;
            }
            else if (item is InformesTreeView)
            {
                return InformesTemplate;
            }
            else if (item is TestTreeView)
            {
                return TestTemplate;
            }
            else if (item is InformeTreeView)
            {
                return InformeTemplate;
            }

            // Return a default template if the item type is not recognized
            return base.SelectTemplate(item, container);
        }
    }
}
