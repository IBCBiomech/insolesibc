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
    public class CalibrarResetCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private RegistroState state;
        public CalibrarResetCommand(RegistroState state) 
        { 
            this.state = state;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            state.fcLeft = null; 
            state.fcRight = null;
            if (state.calibrating)
            {
                state.calibrating = false;
                state.weightsLeft.Clear();
                state.weightsRight.Clear();
            }
        }
    }
}
