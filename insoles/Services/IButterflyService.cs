using insoles.DataHolders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public interface IButterflyService
    {
        void Init();
        Task Calculate(GraphData graphData, out FramePressures[] frames,
            out List<Tuple<double, double>> cps_left, out List<Tuple<double, double>> cps_right);
    }
}
