using Emgu.CV.Ocl;
using insoles.DataHolders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public class FileExtractorService : IFileExtractorService
    {
        public GraphData ExtractCSV(string path)
        {
            using (var reader = new StreamReader(path))
            {
                int headerLines = 5; //Hay un salto de linea al final del header
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
    }
}
