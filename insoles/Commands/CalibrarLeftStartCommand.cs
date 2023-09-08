using insoles.Forms;
using insoles.Model;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace insoles.Commands
{
    public class CalibrarLeftStartCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private RegistroState state;
        public CalibrarLeftStartCommand(RegistroState state) 
        { 
            this.state = state;
            state.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "capturing" || e.PropertyName == "calibratingLeft" || e.PropertyName == "calibratingRight")
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CommandManager.InvalidateRequerySuggested();
                    });
                }
            };
        }
        public bool CanExecute(object? parameter)
        {
            return state.capturing && !state.calibratingLeft && !state.calibratingRight;
        }

        public void Execute(object? parameter)
        {
            state.calibratingLeft = true;
        }
    }
}
