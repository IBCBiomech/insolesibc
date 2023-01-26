using System.Collections.Generic;
using System.Linq;

namespace insoles.Graphs
{
    enum Sensor
    {
        HALLUX,
        TOES,
        MET1,
        MET3,
        MET5,
        ARCH,
        HEEL_L,
        HEEL_R
    }
    internal class Codes
    {
        private float background;
        private float foot;
        private Dictionary<Sensor, float> sensor = new Dictionary<Sensor, float>();
        private Dictionary<Sensor, string> sensorNames = new Dictionary<Sensor, string>();
        public Codes((byte, int)[] frequences)
        {
            background = frequences[0].Item1;
            foot = frequences[1].Item1;
            sensor[Sensor.HALLUX] = frequences[8].Item1;
            sensor[Sensor.TOES] = frequences[3].Item1;
            sensor[Sensor.MET1] = frequences[9].Item1;
            sensor[Sensor.MET3] = frequences[6].Item1;
            sensor[Sensor.MET5] = frequences[2].Item1;
            sensor[Sensor.ARCH] = frequences[4].Item1;
            sensor[Sensor.HEEL_L] = frequences[5].Item1;
            sensor[Sensor.HEEL_R] = frequences[7].Item1;
        }
        public Codes((float, int)[] frequences)
        {
            background = frequences[0].Item1;
            foot = frequences[1].Item1;
            sensor[Sensor.HALLUX] = frequences[8].Item1;
            sensor[Sensor.TOES] = frequences[3].Item1;
            sensor[Sensor.MET1] = frequences[9].Item1;
            sensor[Sensor.MET3] = frequences[6].Item1;
            sensor[Sensor.MET5] = frequences[2].Item1;
            sensor[Sensor.ARCH] = frequences[4].Item1;
            sensor[Sensor.HEEL_L] = frequences[5].Item1;
            sensor[Sensor.HEEL_R] = frequences[7].Item1;
        }
        public float GetCode(Sensor s)
        {
            return sensor[s];
        }
        public float Foot()
        {
            return foot;
        }
        public float Background()
        {
            return background;
        }
        public bool IsSensor(float code)
        {
            return sensor.ContainsValue(code);
        }
        public Sensor GetSensor(float code)
        {
            return sensor.FirstOrDefault(x => x.Value == code).Key;
        }
    }
}
