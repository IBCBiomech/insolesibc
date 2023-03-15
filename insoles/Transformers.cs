using insoles.Graphs;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot.Drawing;
using insoles.Common;
using System.Diagnostics;
using MathNet.Numerics.Data.Text;

namespace insoles
{
    public class Transformers
    {
        public static void transformImage()
        {
            string file = "plantilla.png";
            string path = Helpers.GetFilePath(file);
            Bitmap bmp = new Bitmap(path);
            Matrix<float> sensor_map = Helpers.ImageToMatrix(bmp);
            /*
            sensor_map = Matrix<float>.Build.DenseOfArray(
                new float[,] { 
                    { 10, 10, 0, 0, 10, 10 },
                    { 20, 10, 0, 0 , 10, 20 },
                    {0, 0, 0, 0, 0, 0 },
                    {20, 20, 0, 0, 20, 20 }
                }
                );
            */
            Codes codes = new Codes();
            sensor_map = replaceWithClosestNum(sensor_map, codes);
            checkAllValuesValid(sensor_map, codes);
            sensor_map = removeOutliers(sensor_map, codes);
            DelimitedWriter.Write(Config.INITIAL_PATH + "\\model_heatmap.csv", sensor_map, ",");
            saveBitmap(sensor_map);
        }
        private static void checkAllValuesValid(Matrix<float> matrix, Codes codes)
        {
            int count = 0;
            for(int i = 0; i < matrix.RowCount; i++)
            {
                for(int j = 0; j < matrix.ColumnCount; j++)
                {
                    if(!codes.IsValidCode(matrix[i, j]))
                    {
                        count++;
                    }
                }
            }
            if(count > 0)
            {
                throw new Exception(count + " Invalid values");
            }
        }
        private static void printCode(Matrix<float> matrix, float code)
        {
            var sensorPoints = FindAll(matrix, (value) => value == code);
            (int, int, float) leftPoint = (200, 200, 30);
            foreach (var point in sensorPoints)
            {
                string side;
                if(sameFoot(point, leftPoint, matrix.RowCount))
                {
                    side = "LEFT";
                }
                else
                {
                    side = "RIGHT";
                }
                Trace.WriteLine(point + " " + side);
            }
        }
        public static void saveBitmap(Matrix<float> matrix)
        {
            int height = matrix.RowCount;
            int width = matrix.ColumnCount;
            Bitmap bitmap = new Bitmap(height, width, PixelFormat.Format32bppArgb);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int value = (int)matrix[i, j];
                    bitmap.SetPixel(i, j, Color.FromArgb(255, value, value, value));
                }
            }
            bitmap.Save(Config.INITIAL_PATH + "\\bitmap_heatmap.png", ImageFormat.Png);
        }
        public static Matrix<float> removeOutliers(Matrix<float> map, Codes codes)
        {
            Dictionary<Sensor, float> sensor = codes.sensor;
            float foot = codes.Foot();
            Dictionary<Sensor, List<(int, int, float)>> sensorPoints = new Dictionary<Sensor, List<(int, int, float)>>();
            foreach (KeyValuePair<Sensor, float> code in sensor)
            {
                sensorPoints[code.Key] = FindAll(map, (value) => value == code.Value);
            }
            Dictionary<(int, int, float), (float, float, Sensor)> minDistances = new Dictionary<(int, int, float), (float, float, Sensor)>();
            foreach (Sensor s in sensor.Keys)
            {
                List<(int, int, float)> points = sensorPoints[s];
                foreach ((int, int, float) point in points)
                {
                    float sumDist = 0;
                    int count = 0;
                    foreach ((int, int, float) otherPoint in points)
                    {
                        if (sameFoot(point, otherPoint, map.RowCount))
                        {
                            sumDist += distance(point, otherPoint);
                            count++;
                        }
                    }
                    if(count < 10)
                    {
                        Trace.WriteLine(point + " count = " + count + " total = " + points.Count);
                    }
                    minDistances[point] = (sumDist / count, float.MaxValue, s);
                    foreach (Sensor otherSensor in sensor.Keys)
                    {
                        if (otherSensor != s)
                        {
                            List<(int, int, float)> otherSensorPoints = sensorPoints[otherSensor];
                            float sumDistOtherSensor = 0;
                            int otherCount = 0;
                            foreach ((int, int, float) otherSensorPoint in otherSensorPoints)
                            {
                                if (sameFoot(point, otherSensorPoint, map.RowCount))
                                {
                                    sumDistOtherSensor += distance(point, otherSensorPoint);
                                    otherCount++;
                                }
                            }
                            if (otherCount < 10)
                            {
                                Trace.WriteLine(point + " other count = " + otherCount + " other total = " + otherSensorPoints.Count);
                            }
                            if (otherCount > 0)
                            {
                                float avgDistOtherSensor = sumDistOtherSensor / otherCount;
                                if (avgDistOtherSensor < minDistances[point].Item2)
                                {
                                    minDistances[point] = (minDistances[point].Item1, avgDistOtherSensor, otherSensor);
                                }
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<(int, int, float), (float, float, Sensor)> minDistance in minDistances)
            {
                if (minDistance.Value.Item2 < minDistance.Value.Item1)
                {
                    map[minDistance.Key.Item1, minDistance.Key.Item2] = foot;
                }
            }
            return map;
        }
        private static bool sameFoot((int, int, float) point1, (int, int, float) point2, int rows)
        {
            if(point1.Item1 < rows / 2)
            {
                return point2.Item1 < rows / 2;
            }
            return point2.Item1 > rows / 2;
        }
        private static float distance((int, int, float) point1, (int, int, float) point2)
        {
            return (float)Math.Sqrt(Math.Pow(point1.Item1 - point2.Item1, 2) + Math.Pow(point1.Item2 - point2.Item2, 2));
        }
        public static List<(int, int, float)> FindAll(Matrix<float> matrix, Func<float, bool> func)
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
        private static Matrix<float> replaceWithClosestNum(Matrix<float> map, Codes codes)
        {
            return map.Map(codes.transformValue);
        }
    }
}
