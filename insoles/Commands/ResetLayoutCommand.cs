using insoles.Utilities;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace insoles.Commands
{
    public class ResetLayoutCommand : ICommand
    {
        public string? initialLayout { get; set; }
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
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            ContentControl contentControl = (ContentControl)mainWindow.FindName("Pages");

            if (contentControl != null)
            {
                DockingManager dockingManager = HelperFunctions.FindVisualChild<DockingManager>(contentControl);
                if (dockingManager != null)
                {
                    dockingManager.ResetState();
                }
                else
                {
                    Trace.WriteLine("docking manager null execute");
                }
            }
            else
            {
                Trace.WriteLine("content control null execute");
            }
            
        }
    }
}
