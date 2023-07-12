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
    public class RenombrarFicheroVideo1TestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private DatabaseBridge databaseBridge;
        private Test test;
        public RenombrarFicheroVideo1TestCommand(DatabaseBridge databaseBridge, Test test)
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
                string directory = Path.GetDirectoryName(test.video1);
                string newPath = Path.Combine(directory, text) + ".avi";
                if (File.Exists(newPath))
                {
                    MessageBox.Show("Ya existe un fichero con ese nombre", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    File.Move(test.video1, newPath);
                    test.video1 = newPath;
                    await databaseBridge.UpdateTest(test);
                }
            };
            inputForm.ShowDialog();
        }
    }
}
