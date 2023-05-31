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
            float[] metric_left = new float[left.Count];
            float[] metric_right = new float[right.Count];

            Units units = Units.N;
            Metric metric = Metric.Sum;
            Func<int, float> transformFunc;
            switch (units)
            {
                case Units.mbar:
                    transformFunc = (VALUE_digital) => UnitsConversions.VALUE_mbar(
                        UnitsConversions.ADC_neg(VALUE_digital));
                    break;
                case Units.N:
                    transformFunc = (VALUE_digital) => UnitsConversions.N(
                    UnitsConversions.VALUE_mbar(UnitsConversions.ADC_neg(VALUE_digital)));
                    break;
                default:
                    throw new Exception("ninguna unidad seleccionada");
            }
            switch (metric)
            {
                case Metric.Avg:

                    break;
                case Metric.Sum:

                    break;
            }
            int numSensors = left[0].raw.Count;
            int numPackets = left.Count;
            if (metric == Metric.Avg)
            {
                for (int i = 0; i < numPackets; i++)
                {
                    metric_left[i] = Sum(left[i], transformFunc) / numSensors;
                    metric_right[i] = Sum(right[i], transformFunc) / numSensors;
                }
            }
            else if (metric == Metric.Sum)
            {
                for (int i = 0; i < numPackets; i++)
                {
                    metric_left[i] = Sum(left[i], transformFunc);
                    metric_right[i] = Sum(right[i], transformFunc);
                }
            }
            ResultReady?.Invoke(metric, units, left, right, metric_left, metric_right);
        }
        private float Sum(InsoleData insole, Func<int, float> transformFunc)
        {
            float sum = 0;
            foreach (var digital in insole.raw.Values)
            {
                sum += transformFunc(digital);
            }
            return sum;
        }
    }
}
