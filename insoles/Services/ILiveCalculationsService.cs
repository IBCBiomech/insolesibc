using insoles.Enums;
using insoles.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public interface ILiveCalculationsService
    {
        void ProcessPacket(byte handler, List<InsoleData> data);
        public delegate void ResultEventHandler( 
            List<Dictionary<Sensor, double>> left, List<Dictionary<Sensor, double>> right,
            float[] metricLeft, float[] metricRight);
        public event ResultEventHandler ResultReady;
    }
}
