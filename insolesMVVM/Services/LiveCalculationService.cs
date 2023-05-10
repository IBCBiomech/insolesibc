using CommunityToolkit.Mvvm.Messaging;
using insolesMVVM.Enums;
using insolesMVVM.Helpers;
using insolesMVVM.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insolesMVVM.Services
{
    public class LiveCalculationService : ILiveCalculationService
    {
        private List<InsoleData> left;
        private List<InsoleData> right;

        private byte handlerLeft = 0;
        private byte handlerRight = 1;

        private int counter = 0;
        public LiveCalculationService()
        {
            WeakReferenceMessenger.Default.Register<InsoleMeasuresMessage>(this, onInsoleMeasuresMessageReceived);
        }
        private void onInsoleMeasuresMessageReceived(object sender, InsoleMeasuresMessage args)
        {
            if (args.handler == handlerLeft)
            {
                left = args.measures;
                counter++;
            }
            else if (args.handler == handlerRight)
            {
                right = args.measures;
                counter++;
            }
            if (counter % 2 == 0)
            {
                Calculate();
            }
        }
        private void Calculate()
        {
            Trace.WriteLine("Calculate from LiveCalculationService");
            float[] metric_left = new float[left.Count];
            float[] metric_right = new float[right.Count];

            Units units = Units.mbar;
            Metric metric = Metric.Avg;
            Func<int, float> transformFunc;
            switch (units)
            {
                case Units.mbar:
                    transformFunc = (VALUE_digital) => UnitsConversion.VALUE_mbar(
                        UnitsConversion.ADC_neg(VALUE_digital));
                    break;
                case Units.N:
                    transformFunc = (VALUE_digital) => UnitsConversion.N(
                    UnitsConversion.VALUE_mbar(UnitsConversion.ADC_neg(VALUE_digital)));
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
            LiveCalculationsMessage message = new LiveCalculationsMessage(metric, units,
                left, right, metric_left, metric_right);
            WeakReferenceMessenger.Default.Send(message);
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
