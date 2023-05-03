using mvvm.Enums;
using mvvm.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class InsoleMeasureData
    {
        public Dictionary<Sensor, int> raw { get; set; } = new Dictionary<Sensor, int>();
        public InsoleMeasureData(WisewalkSDK.WisewalkData data, int index) 
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
        public InsoleMeasureData(WisewalkSDK.SoleSensor sole)
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
        public string ToString(List<Sensor> order)
        {
            StringBuilder result = new StringBuilder();
            for(int i = 0; i < order.Count - 1; i++) 
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
                    tf = UnitsConversions.VALUE_mbar_from_VALUE_digital;
                    break;
                case Units.N:
                    tf = UnitsConversions.N_from_VALUE_digital;
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
