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
        public Dictionary<Sensor, float> sensor { get; private set; } = new Dictionary<Sensor, float>();
        private Dictionary<Sensor, string> sensorNames = new Dictionary<Sensor, string>();
        private Dictionary<Sensor, (float, float)> centers = new Dictionary<Sensor, (float, float)>();
        private Dictionary<Sensor, (float, float)> avgDistances2 = new Dictionary<Sensor, (float, float)>();
        private Dictionary<Sensor, float> avgDistancesSquared = new Dictionary<Sensor, float>();
        private Dictionary<Sensor, float> avgDistances = new Dictionary<Sensor, float>();
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
        public Matrix<float> removeOutliers(Matrix<float> map)
        {
            Dictionary<Sensor, List<(int, int, float)>> sensorPoints = new Dictionary<Sensor, List<(int, int, float)>>();
            foreach (KeyValuePair<Sensor, float> code in sensor)
            {
                sensorPoints[code.Key] = FindAll(map, (value) => value == code.Value);
            }
            Dictionary<(int, int, float), (float, float, Sensor)> minDistances = new Dictionary<(int, int, float), (float, float, Sensor)>();
            foreach (Sensor s in sensor.Keys)
            {
                List<(int, int, float)> points = sensorPoints[s];
                foreach((int, int, float) point in points)
                {
                    float sumDist = 0;
                    foreach ((int, int, float) otherPoint in points)
                    {
                        sumDist += distance(point, otherPoint);
                    }
                    minDistances[point] = (sumDist / points.Count, float.MaxValue, s);
                    foreach(Sensor otherSensor in sensor.Keys)
                    {
                        if(otherSensor != s)
                        {
                            List<(int, int, float)> otherSensorPoints = sensorPoints[otherSensor];
                            float sumDistOtherSensor = 0;
                            foreach ((int, int, float) otherSensorPoint in otherSensorPoints)
                            {
                                sumDist += distance(point, otherSensorPoint);
                            }
                            float avgDistOtherSensor = sumDistOtherSensor / otherSensorPoints.Count;
                            if(avgDistOtherSensor < minDistances[point].Item2)
                            {
                                minDistances[point] = (minDistances[point].Item1, avgDistOtherSensor, otherSensor);
                            }
                        }
                    }
                }
            }
            foreach(KeyValuePair< (int, int, float), (float, float, Sensor)> minDistance in minDistances)
            {
                if(minDistance.Value.Item2 < minDistance.Value.Item1)
                {
                    map[minDistance.Key.Item1, minDistance.Key.Item2] = foot;
                }
            }
            return map;
        }
        private float distance((int, int, float) point1, (int, int, float) point2)
        {
            return (float)Math.Sqrt(Math.Pow(point1.Item1 - point2.Item1, 2) + Math.Pow(point1.Item2 - point2.Item2, 2));
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
        public void calculateCenters(Matrix<float> map)
        {
            foreach (KeyValuePair<Sensor, float> code in sensor)
            {
                List<(int, int, float)> elements = FindAll(map, (value) => value == code.Value);
                float rowSum = 0;
                float columnSum = 0;
                foreach(var e in elements)
                {
                    rowSum += e.Item1;
                    columnSum += e.Item2;
                }
                float rowCenter = rowSum / elements.Count;
                float columnCenter = columnSum / elements.Count;
                centers[code.Key] = (rowCenter, columnCenter);
                float rowDistSum = 0;
                float columnDistSum = 0;
                foreach (var e in elements)
                {
                    rowDistSum += Math.Abs(rowCenter - e.Item1);
                    columnDistSum += Math.Abs(columnCenter - e.Item2);
                }
                float rowDistAvg = rowDistSum / elements.Count;
                float columnDistAvg = columnDistSum / elements.Count;
                avgDistances2[code.Key] = (rowDistAvg, columnDistAvg);
                float distSumSquared = 0;
                float distSum = 0;
                foreach (var e in elements)
                {
                    double distSquared = Math.Pow(rowCenter - e.Item1, 2) + Math.Pow(columnCenter - e.Item2, 2);
                    distSumSquared += (float)distSquared;
                    distSum += (float)Math.Sqrt(distSquared);
                }
                float distSumSquaredAvg = distSumSquared / elements.Count;
                avgDistancesSquared[code.Key] = distSumSquaredAvg;
                float distSumAvg = distSum / elements.Count;
                avgDistances[code.Key] = distSumAvg;
            }
            // clear outliers
            const float FACTOR = 2f;
            foreach (KeyValuePair<Sensor, float> code in sensor)
            {
                List<(int, int, float)> elements = FindAll(map, (value) => value == code.Value);

                elements = filterAxis(elements, code.Key);
                
                (float, float) center = centers[code.Key];
                (float, float) averageDistanceAxis = avgDistances2[code.Key];
                float rowCenter = center.Item1;
                float columnCenter = center.Item2;
                float rowDistSum = 0;
                float columnDistSum = 0;
                foreach (var e in elements)
                {
                    rowDistSum += Math.Abs(rowCenter - e.Item1);
                    columnDistSum += Math.Abs(columnCenter - e.Item2);
                }
                float rowDistAvg = rowDistSum / elements.Count;
                float columnDistAvg = columnDistSum / elements.Count;
                avgDistances2[code.Key] = (rowDistAvg, columnDistAvg);
                float distSumSquared = 0;
                float distSum = 0;
                foreach (var e in elements)
                {
                    double distSquared = Math.Pow(rowCenter - e.Item1, 2) + Math.Pow(columnCenter - e.Item2, 2);
                    distSumSquared += (float)distSquared;
                    distSum += (float)Math.Sqrt(distSquared);
                }
                float distSumSquaredAvg = distSumSquared / elements.Count;
                avgDistancesSquared[code.Key] = distSumSquaredAvg;
                float distSumAvg = distSum / elements.Count;
                avgDistances[code.Key] = distSumAvg;
            }
        }
        private List<(int, int, float)> filterAxis(List<(int, int, float)> elements, Sensor sensor)
        {
            const float FACTOR = 2f;
            (float, float) center = centers[sensor];
            (float, float) averageDistanceAxis = avgDistances2[sensor];
            float rowCenter = center.Item1;
            float columnCenter = center.Item2;
            elements = elements.Where(((int, int, float) e) =>
            {
                float distanceRow = Math.Abs(e.Item1 - rowCenter);
                float distanceCol = Math.Abs(e.Item2 - columnCenter);
                return distanceRow < averageDistanceAxis.Item1 * FACTOR && distanceCol < averageDistanceAxis.Item2 * FACTOR;
            }).ToList();
            return elements;
        }
        public float transformValueProximity(int row, int col, float value)
        {
            if(value == foot || value == background)
            {
                return value;
            }
            Sensor sensor = this.sensor.FirstOrDefault(x => x.Value == value).Key;
            (float, float) center = centers[sensor];
            float distSquared = (float)Math.Pow(row - center.Item1, 2) + (float)Math.Pow(col - center.Item2, 2);
            float distSquaredAvg = avgDistancesSquared[sensor];
            float dist = (float)Math.Sqrt(distSquared);
            float distAvg = avgDistances[sensor];
            const float FACTOR = 1.5f;
            if (dist > distAvg * FACTOR)
            {
                return foot;
            }
            return value;
        }
        public float transformValueProximityAxis(int row, int col, float value)
        {
            if (value == foot || value == background)
            {
                return value;
            }
            Sensor sensor = this.sensor.FirstOrDefault(x => x.Value == value).Key;
            (float, float) center = centers[sensor];
            float distanceRow = Math.Abs(row - center.Item1);
            float distanceCol = Math.Abs(col - center.Item2);
            (float, float) averageDistance = avgDistances2[sensor];
            const float FACTOR = 2f;
            if(distanceRow > averageDistance.Item1 * FACTOR || distanceCol > averageDistance.Item2 * FACTOR)
            {
                return foot;
            }
            return value;
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
                    result = value;
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
