using insoles.Enums;
using insoles.Messages;
using OpenCvSharp;
using System.Collections.Generic;

namespace insoles.Services
{
    public interface ISaveService
    {
        public bool recording { get; }
        void Start(int fps, Size size);
        void Stop();
        void AppendVideo(Mat frame);
        void AppendCSV(List<Dictionary<Sensor, double>> left, List<Dictionary<Sensor, double>> right,
                    float[] metricLeft, float[] metricRight);
    }
}
