using insoles.States;
using insoles.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class TimelineFastForwardCommand : ICommand
    {
        private AnalisisState state;
        private TimeLine timeLine;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public TimelineFastForwardCommand(AnalisisState state, TimeLine timeLine)
        {
            this.state = state;
            this.timeLine = timeLine;
        }

        public bool CanExecute(object? parameter)
        {
            return state.test != null;
        }

        public void Execute(object? parameter)
        {
            timeLine.FastForward();
        }
    }
}
