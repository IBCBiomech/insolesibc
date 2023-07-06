using insoles.DataHolders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public interface IFileExtractorService
    {
        VariablesData ExtractVariables(string path);
        Task<GraphData> ExtractCSV(string path);
    }
}
