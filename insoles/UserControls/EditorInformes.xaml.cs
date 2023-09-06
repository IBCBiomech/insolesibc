using ScottPlot.Palettes;
using Syncfusion.DocIO.DLS;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging; // Add reference for 'ReachFramework'.


namespace insoles.UserControls
{
    /// <summary>
    /// Interaction logic for EditorInformes.xaml
    /// </summary>
    public partial class EditorInformes : UserControl
    {
        private string docpath = @"C:\Users\Deva\insoles\20230903-18-22-31-139.docx";
        private string rtfpath = @"C:\Users\Deva\insoles\20230903-18-22-31-139.rtf";


        public EditorInformes()
        {
            InitializeComponent();

            
        }
        
        public void CargarPath(string rtfpath)
        {
            this.sfRichTextAdv.LoadAsync(rtfpath);
        }
    }
}
