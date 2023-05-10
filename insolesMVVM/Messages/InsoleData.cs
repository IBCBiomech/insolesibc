using insolesMVVM.Enums;
using insolesMVVM.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace insolesMVVM.Messages
{
    public class InsoleData
    {
        public Dictionary<Sensor, int> raw { get; set; } = new Dictionary<Sensor, int>();
        public InsoleData(WisewalkSDK.WisewalkData data, int index)
        {
            WisewalkSDK.SoleSensor sole = data.Sole[index];
            raw[Sensor.Hallux] = sole.hallux;
            raw[Sensor.Toes] = sole.toes;
            raw[Sensor.Met1] = sole.met_1;
            raw[Sensor.Met3] = sole.met_3;
            raw[Sensor.Met5] = sole.met_5;
            raw[Sensor.Arch] = sole.arch;
            raw[Sensor.HeelL] = sole.heel_L;
            raw[Sensor.HeelR] = sole.heel_R;
        }
        public InsoleData(WisewalkSDK.SoleSensor sole)
        {
            raw[Sensor.Hallux] = sole.hallux;
            raw[Sensor.Toes] = sole.toes;
            raw[Sensor.Met1] = sole.met_1;
            raw[Sensor.Met3] = sole.met_3;
            raw[Sensor.Met5] = sole.met_5;
            raw[Sensor.Arch] = sole.arch;
            raw[Sensor.HeelL] = sole.heel_L;
            raw[Sensor.HeelR] = sole.heel_R;
        }
        public InsoleData(Random random)
        {
            foreach(Sensor sensor in Enum.GetValues(typeof(Sensor)))
            {
                raw[sensor] = random.Next(4095);
            }
        }
        public string ToString(List<Sensor> order)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < order.Count - 1; i++)
            {
                result.Append(raw[order[i]].ToString() + " ");
            }
            result.Append(raw[order[order.Count - 1]].ToString());
            return result.ToString();
        }
        public string ToString(List<Sensor> order, Units units)
        {
            Func<int, float> tf;
            switch (units)
            {
                case Units.mbar:
                    tf = UnitsConversion.VALUE_mbar_from_VALUE_digital;
                    break;
                case Units.N:
                    tf = UnitsConversion.N_from_VALUE_digital;
                    break;
                default:
                    throw new Exception("InsoleMeasureData public string ToString(List<Sensor> order, Units units) trying to convert to a invalid unit");
            }
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < order.Count - 1; i++)
            {
                result.Append(tf(raw[order[i]]).ToString() + " ");
            }
            result.Append(tf(raw[order[order.Count - 1]]).ToString());
            return result.ToString();
        }
    }
}
