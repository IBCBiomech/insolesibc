using insoles.Messages;
using OpenCvSharp;
using System.Collections.Generic;

namespace insoles.Services
{
    public interface ISaveService
    {
        void Start(int fps, Size size);
        void Stop();
        void AppendVideo(Mat frame);
        void AppendCSV(List<InsoleData> left, List<InsoleData> right,
                    float[] metricLeft, float[] metricRight);
    }
}
