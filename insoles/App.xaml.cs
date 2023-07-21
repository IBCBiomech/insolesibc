using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace insoles
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // MjQzOTU3MUAzMjMxMmUzMTJlMzMzNURNS2pOa1Z5L2x4Y0NSSys5V1RvdG0wMFJFQnVmY2xKOE40NTlzZ2JFVkE9
            // MjUzMjQ4M0AzMjMyMmUzMDJlMzBHNWlFdmkxZ0ZsTHIrZGZ2cDhnbDNWL2pJbzd5VWdlejFjdXEzbFZCMzRZPQ==

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjUzMjQ4M0AzMjMyMmUzMDJlMzBHNWlFdmkxZ0ZsTHIrZGZ2cDhnbDNWL2pJbzd5VWdlejFjdXEzbFZCMzRZPQ==\r\n");
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string userName = Environment.UserName;
            string path = "C:\\Users\\" + userName + "\\insoles";

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    Trace.WriteLine("Creada carpeta insoles");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al crear la carpeta insoles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                Trace.WriteLine("Creada carpeta insoles ya existe");
            }

            // Aquí puedes continuar con la lógica de inicio de tu aplicación.
        }
    }
}
