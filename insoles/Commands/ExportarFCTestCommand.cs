using insoles.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace insoles.Commands
{
    public class ExportarFCTestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private Test test;
        public ExportarFCTestCommand(Test test)
        {
            this.test = test;
        }

        public bool CanExecute(object? parameter)
        {
            return test.csv != null;
        }

        public void Execute(object? parameter)
        {
            Task.Run(() => TransformCSV(test));
        }
        private async Task TransformCSV(Test test)
        {
            using (var reader = new StreamReader(Environment.ExpandEnvironmentVariables(test.csv)))
            {
                int headerLines = 5; //Hay un salto de linea al final del header
                try
                {
                    string config = reader.ReadLine();
                    Dictionary<string, string> variables = JsonConvert.DeserializeObject<Dictionary<string, string>>(config);
                    if (!variables.ContainsKey("fc"))
                    {
                        MessageBox.Show("El fichero no contiene fc");
                    }
                    else
                    {
                        StringBuilder dataHolder = new StringBuilder();
                        float fc = float.Parse(variables["fc"], CultureInfo.InvariantCulture);
                        for(int i = 0; i < headerLines; i++)
                        {
                            dataHolder.AppendLine(reader.ReadLine());
                        }
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] numbers = line.Split(" ");
                            for (int i = 3; i < numbers.Length; i++)
                            {
                                float currentNumber = float.Parse(numbers[i], CultureInfo.InvariantCulture);
                                float multipliedNumber = currentNumber * fc;
                                numbers[i] = multipliedNumber.ToString("F2", CultureInfo.InvariantCulture);
                            }
                            dataHolder.AppendLine(string.Join(" ", numbers));
                        }
                        Trace.WriteLine(test.csv);
                        string originalPath = Environment.ExpandEnvironmentVariables(test.csv);
                        Trace.WriteLine(originalPath);
                        string originalFileName = Path.GetFileNameWithoutExtension(originalPath);
                        string originalExtension = Path.GetExtension(originalPath);

                        string newFileName = originalFileName + "fc" + originalExtension;
                        string filePath = Path.Combine(Path.GetDirectoryName(originalPath), newFileName);
                        await File.WriteAllTextAsync(filePath, dataHolder.ToString());
                    }
                }
                catch (JsonReaderException)
                {
                    MessageBox.Show("El fichero no contiene header de variables");
                }
            }
        }
    }
}
