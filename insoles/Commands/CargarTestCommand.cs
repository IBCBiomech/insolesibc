using insoles.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace insoles.Commands
{
    public class CargarTestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private Test test;
        private float peso;
        public CargarTestCommand(Test test, float peso)
        {
            this.test = test;
            this.peso = peso;
        }

        public bool CanExecute(object? parameter)
        {
            return test.csv != null;
        }

        public void Execute(object? parameter)
        {
            ((MainWindow)Application.Current.MainWindow).analisisState.test = test;
            ((MainWindow)Application.Current.MainWindow).analisisState.peso = peso;
        }
    }
}
