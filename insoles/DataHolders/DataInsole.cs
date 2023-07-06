using insoles.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.DataHolders
{
    public class DataInsole
    {
        public Dictionary<Sensor, double> pressures { get; private set; } = new Dictionary<Sensor, double>();
        public double getPressure(Sensor sensor)
        {
            return pressures[sensor];
        }
        public double this[Sensor sensor]
        {
            get { return pressures[sensor]; }
            set { pressures[sensor] = value; }
        }
        public DataInsole(string[] values, int firstIndex)
        {
            pressures[Sensor.Arch] = double.Parse(values[firstIndex], CultureInfo.InvariantCulture);
            pressures[Sensor.Hallux] = double.Parse(values[firstIndex + 1], CultureInfo.InvariantCulture);
            pressures[Sensor.HeelL] = double.Parse(values[firstIndex + 2], CultureInfo.InvariantCulture);
            pressures[Sensor.HeelR] = double.Parse(values[firstIndex + 3], CultureInfo.InvariantCulture);
            pressures[Sensor.Met1] = double.Parse(values[firstIndex + 4], CultureInfo.InvariantCulture);
            pressures[Sensor.Met3] = double.Parse(values[firstIndex + 5], CultureInfo.InvariantCulture);
            pressures[Sensor.Met5] = double.Parse(values[firstIndex + 6], CultureInfo.InvariantCulture);
            pressures[Sensor.Toes] = double.Parse(values[firstIndex + 7], CultureInfo.InvariantCulture);
        }
        public DataInsole()
        {
            pressures[Sensor.Arch] = 0;
            pressures[Sensor.Hallux] = 0;
            pressures[Sensor.HeelL] = 0;
            pressures[Sensor.HeelR] = 0;
            pressures[Sensor.Met1] = 0;
            pressures[Sensor.Met3] = 0;
            pressures[Sensor.Met5] = 0;
            pressures[Sensor.Toes] = 0;
        }
        public int totalPressure
        {
            get
            {
                int result = 0;
                foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    result += (int)pressures[sensor];
                }
                return result;
            }
        }
        public override string ToString()
        {
            string result = "";
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                result += sensor.ToString() + " " + pressures[sensor] + ", ";
            }
            return result;
        }
    }
}
