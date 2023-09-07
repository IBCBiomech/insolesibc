using insoles.Forms;
using insoles.Model;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class CalibrarRightStartCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private RegistroState state;
        public CalibrarRightStartCommand(RegistroState state) 
        { 
            this.state = state;
        }
        public bool CanExecute(object? parameter)
        {
            return state.capturing && !state.calibratingLeft && !state.calibratingRight;
        }

        public void Execute(object? parameter)
        {
            state.calibratingRight = true;
        }
    }
}
