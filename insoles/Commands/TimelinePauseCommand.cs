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
    public class TimelinePauseCommand : ICommand
    {
        private AnalisisState state;
        private TimeLine timeLine;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public TimelinePauseCommand(AnalisisState state, TimeLine timeLine)
        {
            this.state = state;
            this.timeLine = timeLine;
        }

        public bool CanExecute(object? parameter)
        {
            return state.test != null && !state.paused;
        }

        public void Execute(object? parameter)
        {
            timeLine.Pause();
        }
    }
}
