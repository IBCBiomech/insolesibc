using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using insoles.Forms;
using insoles.Model;
using insoles.States;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public ResetLayoutCommand() 
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.Loaded += (s, e) =>
            {
                ContentControl contentControl = (ContentControl)mainWindow.FindName("Pages");

                if (contentControl != null)
                {
                    Trace.WriteLine("pages found");
                    mainWindow.viewChanged += (s, e) => // Esto va con vista de retraso
                    //contentControl.DataContextChanged += (s, e) => // Cambiar esto por un evento que se ejecute cuando cambia de vista
                    {
                        DockingManager dockingManager = HelperFunctions.FindVisualChild<DockingManager>(contentControl);
                        if (dockingManager == null)
                        {
                            initialLayout = null;
                            Trace.WriteLine("docking manager not found");
                        }
                        else
                        {
                            Trace.WriteLine("docking manager found");
                            var layoutSerializer = new XmlLayoutSerializer(dockingManager);
                            using (var stream = new StringWriter())
                            {
                                layoutSerializer.Serialize(stream);
                                initialLayout = stream.ToString();
                            }
                        }
                    };
                }
                else
                {
                    Trace.WriteLine("Pages not found");
                }
            };
        }

        public bool CanExecute(object? parameter)
        {
            return initialLayout != null;
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
                    var layoutSerializer = new XmlLayoutSerializer(dockingManager);
                    Trace.WriteLine(initialLayout);
                    layoutSerializer.Deserialize(new StringReader(initialLayout));
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
