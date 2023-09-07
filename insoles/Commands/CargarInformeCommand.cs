using insoles.Model;
using insoles.Utilities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace insoles.Commands
{
    public class CargarInformeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private InformeFile informe;
        public CargarInformeCommand(InformeFile informe)
        {
            this.informe = informe;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            string path = informe.path;
            Trace.WriteLine(path);

            // Todo esto es para
            string fullpath = Environment.ExpandEnvironmentVariables(path);
            
            // Rutina para cambiar de docx a rtf
            FileStream fileStreamPath = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            //Loads an existing document
            WordDocument document = new WordDocument(fullpath, FormatType.Docx);


            // Todo esto es para sacar el fichero sin extensión y ponerle rtf 

            string directory = Path.GetDirectoryName(fullpath);
            string rtffile = Path.GetFileNameWithoutExtension(fullpath);
            string rtfpath = directory + "\\" + rtffile;
            rtfpath = rtfpath + ".rtf";


            //Saves the Word document as RTF file
            document.Save(rtfpath, FormatType.Rtf);
            //Closes the document
            document.Close();

            // Se lo pasamos al usercontrol EditorInformes

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.informesState.Path = rtfpath;
            

            

        }
            
    }
}
