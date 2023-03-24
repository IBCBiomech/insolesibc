using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace insoles.Graphs
{
    public enum Sensor
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
    public class Codes
    {
        private float background;
        private float foot;
        public const int FOOT_BUTERFLY = 128;
        public Dictionary<Sensor, float> sensor { get; private set; } = new Dictionary<Sensor, float>();
        private Dictionary<Sensor, string> sensorNames = new Dictionary<Sensor, string>();
        private Dictionary<Sensor, (float, float)> centers = new Dictionary<Sensor, (float, float)>();
        private Dictionary<Sensor, (float, float)> avgDistances2 = new Dictionary<Sensor, (float, float)>();
        private Dictionary<Sensor, float> avgDistancesSquared = new Dictionary<Sensor, float>();
        private Dictionary<Sensor, float> avgDistances = new Dictionary<Sensor, float>();
        // dibujo pie
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
        // Dibujo plantilla
        public Codes()
        {
            background = 255;
            foot = 0;
            sensor[Sensor.HALLUX] = 10;
            sensor[Sensor.TOES] = 20;
            sensor[Sensor.MET1] = 30;
            sensor[Sensor.MET3] = 40;
            sensor[Sensor.MET5] = 50;
            sensor[Sensor.ARCH] = 60;
            sensor[Sensor.HEEL_L] = 70;
            sensor[Sensor.HEEL_R] = 80;
        }
        public List<(int, int, float)> FindAll(Matrix<float> matrix, Func<float, bool> func)
        {
            List<(int, int, float)> result = new List<(int, int, float)>();
            foreach (var tuple in matrix.EnumerateIndexed())
            {
                if (func(tuple.Item3))
                {
                    result.Add(tuple);
                }
            }
            return result;
        }
        public float transformValue(float value)
        {
            float diff = float.MaxValue;
            float result = value;
            float currentDiff;

            currentDiff = Math.Abs(value - background);
            if (currentDiff < diff)
            {
                result = background;
                diff = currentDiff;
            }

            foreach (float code in sensor.Values)
            {
                currentDiff = Math.Abs(value - code);
                if(currentDiff < diff)
                {
                    result = code;
                    diff = currentDiff;
                }
            }

            currentDiff = Math.Abs(value - foot);
            if (currentDiff < diff)
            {
                result = foot;
                diff = currentDiff;
            }

            return result;
        }
        public float transformToBackgroundOrFoot(float value)
        {
            float diff = float.MaxValue;
            float result = value;
            float currentDiff;

            currentDiff = Math.Abs(value - background);
            if (currentDiff < diff)
            {
                result = background;
                diff = currentDiff;
            }

            foreach (float code in sensor.Values)
            {
                currentDiff = Math.Abs(value - code);
                if (currentDiff < diff)
                {
                    result = FOOT_BUTERFLY;
                    diff = currentDiff;
                }
            }

            currentDiff = Math.Abs(value - foot);
            if (currentDiff < diff)
            {
                result = FOOT_BUTERFLY;
                diff = currentDiff;
            }

            return result;
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
