using Emgu.CV.Ocl;
using insoles.DataHolders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace insoles.Services
{
    public class FileExtractorService : IFileExtractorService
    {
        public async Task<GraphData> ExtractCSV(string path)
        {
            using (var reader = new StreamReader(Environment.ExpandEnvironmentVariables(path)))
            {
                int headerLines = 5; //Hay un salto de linea al final del header
                try
                {
                    string config = reader.ReadLine();
                    Dictionary<string, string> variables = JsonConvert.DeserializeObject<Dictionary<string, string>>(config);
                }
                catch(JsonReaderException)
                {
                    headerLines--; //No habia header de variables
                }
                string header = "";
                for (int _ = 0; _ < headerLines; _++)
                {
                    header += reader.ReadLine() + "\n";
                }
                FrameDataFactory factory = new FrameDataFactoryInsoles();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    factory.addLine(line);
                }
                return factory.getData();
            }
        }

        public VariablesData ExtractVariables(string path)
        {
            using (var reader = new StreamReader(Environment.ExpandEnvironmentVariables(path)))
            {
                try
                {
                    string config = reader.ReadLine();
                    Dictionary<string, string> variables = JsonConvert.DeserializeObject<Dictionary<string, string>>(config);
                    return new VariablesData(variables);
                }
                catch (JsonReaderException)
                {
                    return new VariablesData();
                }
            }
        }
    }
}
