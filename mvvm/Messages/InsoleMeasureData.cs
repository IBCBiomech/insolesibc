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
        public int adcNeg(Sensor sensor)
        {
            return 4095 - raw[sensor];
        }
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
    }
}
