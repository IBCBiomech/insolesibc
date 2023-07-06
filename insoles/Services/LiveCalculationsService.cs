using insoles.Enums;
using insoles.Messages;
using insoles.Model;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace insoles.Services
{
    public class LiveCalculationsService : ILiveCalculationsService
    {
        private List<InsoleData> left;
        private List<InsoleData> right;

        private byte handlerLeft = 0;
        private byte handlerRight = 1;

        private int counter = 0;

        private ObservableCollection<InsoleModel> insoles;
        private IApiService apiService;
        public LiveCalculationsService(ObservableCollection<InsoleModel> insoles, IApiService apiService)
        {
            this.insoles = insoles;
            this.apiService = apiService;
        }

        public event ILiveCalculationsService.ResultEventHandler ResultReady;

        public void ProcessPacket(byte handler, List<InsoleData> data)
        {
            string mac = apiService.GetMac(handler);
            InsoleModel insole = insoles.Where((i) => i.MAC == mac).FirstOrDefault();
            if (insole != null)
            {
                if(insole.side == Side.Left)
                {
                    left = data;
                    counter++;
                }
                else if (insole.side == Side.Right)
                {
                    right = data;
                    counter++;
                }
            }
            if (counter % 2 == 0 && left != null && right != null)
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
                    N_left_i[sensor] = 2 * left[i].N(sensor); // Para que salgan bien los N
                    N_right_i[sensor] = 2 * right[i].N(sensor); // Para que salgan bien los N
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
