using insoles.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public class CodesService : ICodesService
    {
        private float background;
        private float foot;
        private Dictionary<Sensor, float> sensor { get; set; } = new Dictionary<Sensor, float>();
        public CodesService()
        {
            background = 255;
            foot = 0;
            sensor[Sensor.Hallux] = 10;
            sensor[Sensor.Toes] = 20;
            sensor[Sensor.Met1] = 30;
            sensor[Sensor.Met3] = 40;
            sensor[Sensor.Met5] = 50;
            sensor[Sensor.Arch] = 60;
            sensor[Sensor.HeelL] = 70;
            sensor[Sensor.HeelR] = 80;
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
        public bool IsValidCode(float code)
        {
            return code == background || code == foot || sensor.ContainsValue(code);
        }
        public Sensor GetSensor(float code)
        {
            return sensor.FirstOrDefault(x => x.Value == code).Key;
        }
    }
}
