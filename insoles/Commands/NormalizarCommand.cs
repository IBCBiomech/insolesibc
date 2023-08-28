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
    public class NormalizarCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private RegistroState state;
        public NormalizarCommand(RegistroState state) 
        { 
            this.state = state;
        }
        public bool CanExecute(object? parameter)
        {
            return state.capturing && !state.normalizing;
        }

        public void Execute(object? parameter)
        {
            state.normalizing = true;
        }
    }
}
