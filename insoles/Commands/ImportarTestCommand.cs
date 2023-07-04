using insoles.Forms;
using insoles.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace insoles.Commands
{
    public class ImportarTestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private Test test;
        public ImportarTestCommand(Test test)
        {
            this.test = test;
        }

        public bool CanExecute(object? parameter)
        {
            return test.csv == null && test.video1 == null && test.video2 == null;
        }

        public void Execute(object? parameter)
        {
            Test test = parameter as Test;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Text and AVI Files (*.txt;*.avi)|*.txt;*.avi";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string[] selectedFiles = openFileDialog.FileNames;

                List<string> txtFiles = new List<string>();
                List<string> aviFiles = new List<string>();

                if (selectedFiles.Length > 3)
                {
                    MessageBox.Show("Demasiados ficheros");
                    return;
                }
                foreach(string file in selectedFiles)
                {
                    string extension = Path.GetExtension(file);
                    if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        txtFiles.Add(file);
                    }
                    else if (extension.Equals(".avi", StringComparison.OrdinalIgnoreCase))
                    {
                        aviFiles.Add(file);
                    }
                }
                if(txtFiles.Count == 0 || txtFiles.Count > 1) 
                {
                    MessageBox.Show("Uno y solo uno de los ficheros debe ser txt");
                    return;
                }
                if(aviFiles.Count > 2)
                {
                    MessageBox.Show("No puede haber mas de 2 ficheros de video");
                    return;
                }
                test.csv = txtFiles[0];
                FileInfo csvInfo = new FileInfo(txtFiles[0]);
                test.Date = csvInfo.LastWriteTime;
                if (aviFiles.Count >= 1)
                    test.video1 = aviFiles[0];
                if(aviFiles.Count >= 2)
                    test.video2 = aviFiles[1];
                ((MainWindow)Application.Current.MainWindow).databaseBridge.UpdateTest(test);
            }
        }
    }
}
