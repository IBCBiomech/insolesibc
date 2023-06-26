using insoles.Enums;
using insoles.Messages;
using insoles.Utilities;
using System;
using System.Collections.Generic;

namespace insoles.Services
{
    public class LiveCalculationsService : ILiveCalculationsService
    {
        private List<InsoleData> left;
        private List<InsoleData> right;

        private byte handlerLeft = 0;
        private byte handlerRight = 1;

        private int counter = 0;
        public LiveCalculationsService()
        {
            
        }

        public event ILiveCalculationsService.ResultEventHandler ResultReady;

        public void ProcessPacket(byte handler, List<InsoleData> data)
        {
            if (handler == handlerLeft)
            {
                left = data;
                counter++;
            }
            else if (handler == handlerRight)
            {
                right = data;
                counter++;
            }
            if (counter % 2 == 0)
            {
                Calculate();
            }
        }
        private void Calculate()
        {
            List<Dictionary<Sensor, double>> N_left = new();
            List<Dictionary<Sensor, double>> N_right = new();
            float[]? metric_left = new float[left.Count];
            float[]? metric_right = new float[right.Count];

            int numPackets = left.Count;
            for (int i = 0; i < numPackets; i++)
            {
                Dictionary<Sensor, double> N_left_i = new();
                Dictionary<Sensor, double> N_right_i = new();
                metric_left[i] = 0;
                metric_right[i] = 0;
                foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    N_left_i[sensor] = left[i].N(sensor);
                    N_right_i[sensor] = right[i].N(sensor);
                    metric_left[i] += (float)N_left_i[sensor];
                    metric_right[i] += (float)N_right_i[sensor];
                }
                N_left.Add(N_left_i);
                N_right.Add(N_right_i);
            }
            ResultReady?.Invoke(N_left, N_right, metric_left, metric_right);
        }
    }
}
