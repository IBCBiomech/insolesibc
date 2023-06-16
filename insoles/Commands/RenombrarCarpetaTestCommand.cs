using insoles.Forms;
using insoles.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace insoles.Commands
{
    public class RenombrarCarpetaTestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Test test = parameter as Test;
            TextInputForm inputForm = new TextInputForm();
            inputForm.enterEvent += async (s, text) =>
            {
                Trace.WriteLine(text);
                test.Nombre = text;
                await ((MainWindow)Application.Current.MainWindow).databaseBridge.UpdateTest(test);
            };
            inputForm.ShowDialog();
        }
    }
}
