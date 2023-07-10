using System;

namespace insoles.DataHolders
{
    public class FramePressures
    {
        public static double maxPressure { get; private set; } = 0;
        public int frame { get; private set; }
        public Tuple<double, double>? totalCenter { get; private set; }
        public Tuple<double, double>? centerLeft { get; private set; }
        public Tuple<double, double>? centerRight { get; private set; }
        public double totalPressure { get; private set; }
        public FramePressures(int frame, Tuple<double, double>? centerLeft, Tuple<double, double>? centerRight, double total_pressure_left, double total_pressure_right)
        {
            this.frame = frame;
            this.centerLeft = centerLeft;
            this.centerRight = centerRight;
            totalPressure = total_pressure_left + total_pressure_right;
            if (totalPressure > maxPressure)
            {
                maxPressure = totalPressure;
            }
            if (centerLeft == null && centerRight == null)
            {
                totalCenter = null;
            }
            else if (centerLeft == null)
            {
                totalCenter = centerRight;
            }
            else if (centerRight == null)
            {
                totalCenter = centerLeft;
            }
            else
            {
                double row = (total_pressure_left * centerLeft.Item1 + total_pressure_right * centerRight.Item1) / (total_pressure_left + total_pressure_right);
                double col = (total_pressure_left * centerLeft.Item2 + total_pressure_right * centerRight.Item2) / (total_pressure_left + total_pressure_right);
                totalCenter = new Tuple<double, double>(row, col);
            }
        }
        // Nuevo fichero
        public static void Reset()
        {
            maxPressure = 0;
        }
    }
}
