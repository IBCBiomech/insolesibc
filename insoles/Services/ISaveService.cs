using Emgu.CV;
using insoles.Enums;
using insoles.Messages;
using insoles.Model;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace insoles.Services
{
    public interface ISaveService
    {
        void Start(ICameraService cameraService);
        Test Stop();
        void AppendVideo(Mat frame, int index);
        void AppendCSV(List<Dictionary<Sensor, double>> left, List<Dictionary<Sensor, double>> right,
                    float[] metricLeft, float[] metricRight);
    }
}
