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
    public class RenombrarFicheroVideo2TestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private DatabaseBridge databaseBridge;
        public RenombrarFicheroVideo2TestCommand(DatabaseBridge databaseBridge)
        {
            this.databaseBridge = databaseBridge;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            TestTreeView testTreeView = parameter as TestTreeView;
            Test test = testTreeView.testDB;
            TextInputForm inputForm = new TextInputForm();
            Window mainWindow = Application.Current.MainWindow;
            inputForm.Left = mainWindow.Left + mainWindow.Width * 0.2;
            inputForm.Top = mainWindow.Top + mainWindow.Height * 0.85;
            inputForm.enterEvent += async (s, text) =>
            {
                string directory = Path.GetDirectoryName(test.video2);
                string newPath = Path.Combine(directory, text) + ".avi";
                if (File.Exists(newPath))
                {
                    MessageBox.Show("Ya existe un fichero con ese nombre", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    File.Move(test.video2, newPath);
                    test.video2 = newPath;
                    await databaseBridge.UpdateTest(test);
                }
            };
            inputForm.ShowDialog();
        }
    }
}
