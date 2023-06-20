

using insoles.Enums;
using System.Collections.Generic;
using System;
using insoles.DataHolders;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace insoles.Services
{
    public interface IPressureMapService
    {
        Task<Dictionary<UserControls.Metric, Matrix<float>>> CalculateMetrics(GraphData graphData);
        Task<List<Matrix<float>>> CalculateLive(GraphData graphData);
    }
}
