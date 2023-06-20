using insoles.Enums;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;

namespace insoles.Services
{
    public interface IPlantillaService
    {
        Matrix<float> sensor_map { get; }
        int getLength(int index);
        Dictionary<Sensor, List<Tuple<int, int>>> CalculateSensorPositionsLeft();
        Dictionary<Sensor, List<Tuple<int, int>>> CalculateSensorPositionsRight();
        List<Tuple<int, int>> CalculateFootPositionsLeft();
        List<Tuple<int, int>> CalculateFootPositionsRight();
    }
}
