//#define DEBUG

using CommunityToolkit.Mvvm.Messaging;
using mvvm.Enums;
using mvvm.Helpers;
using mvvm.Messages;
using mvvm.Services.Interfaces;
using OpenCvSharp.Internal.Vectors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Services
{
    public class LiveDataCalculationsService : ILiveDataCalculationsService
    {
        private List<InsoleData> left;
        private List<InsoleData> right;

        private byte handlerLeft = 0;
        private byte handlerRight = 1;

        private int counter = 0;
        public LiveDataCalculationsService() 
        {
            WeakReferenceMessenger.Default.Register<InsoleMeasuresMessage>(this, onInsoleMeasuresMessageReceived);
        }
        private void onInsoleMeasuresMessageReceived(object sender, InsoleMeasuresMessage args) 
        {
#if DEBUG
            Trace.WriteLine("onInsoleMeasuresMessageReceived from LiveDataCalculationsService");
#endif
            if (args.handler == handlerLeft)
            {
                left = args.measures;
                counter++;
            }
            else if(args.handler == handlerRight)
            {
                right = args.measures;
                counter++;
            }
            if(counter % 2 == 0)
            {
                Calculate();
            }
        }
        private void Calculate()
        {
#if DEBUG
            Trace.WriteLine("Calculate from LiveDataCalculationsService");
#endif
            float[] metric_left = new float[left.Count];
            float[] metric_right = new float[right.Count];

            Units units = Units.mbar;
            Metric metric = Metric.Avg;
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
            else if(metric == Metric.Sum)
            {
                for (int i = 0; i < numPackets; i++)
                {
                    metric_left[i] = Sum(left[i], transformFunc);
                    metric_right[i] = Sum(right[i], transformFunc);
                }
            }
            LiveDataCalculationsMessage message = new LiveDataCalculationsMessage(metric, units,
                left, right, metric_left, metric_right);
            WeakReferenceMessenger.Default.Send(message);
        }
        private float Sum(InsoleData insole, Func<int, float> transformFunc)
        {
            float sum = 0;
            foreach(var digital in insole.raw.Values)
            {
                sum += transformFunc(digital);
            }
            return sum;
        }
    }
}
