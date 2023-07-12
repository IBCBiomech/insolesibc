using insoles.Forms;
using insoles.Model;
using insoles.States;
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
    public class RenombrarFicheroCSVTestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private DatabaseBridge databaseBridge;
        private Test test;
        public RenombrarFicheroCSVTestCommand(DatabaseBridge databaseBridge, Test test)
        {
            this.databaseBridge = databaseBridge;
            this.test = test;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            TextInputForm inputForm = new TextInputForm();
            Window mainWindow = Application.Current.MainWindow;
            inputForm.Left = mainWindow.Left + mainWindow.Width * 0.2;
            inputForm.Top = mainWindow.Top + mainWindow.Height * 0.85;
            inputForm.enterEvent += async (s, text) =>
            {
                string directory = Path.GetDirectoryName(test.csv);
                string newPath = Path.Combine(directory, text) + ".txt";
                if (File.Exists(newPath))
                {
                    MessageBox.Show("Ya existe un fichero con ese nombre", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    File.Move(test.csv, newPath);
                    test.csv = newPath;
                    await databaseBridge.UpdateTest(test);
                }
            };
            inputForm.ShowDialog();
        }
    }
}
