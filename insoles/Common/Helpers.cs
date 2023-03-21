using System.Windows.Media;
using System.Windows;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using MathNet.Numerics.LinearAlgebra;
using System.Drawing;
using Color = System.Drawing.Color;
using System.IO;

namespace insoles.Common
{
    public static class Helpers
    {
        // Devuelve el primer descendiente de tipo T de un objeto en el arbol xaml
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        public static float NextFloat(float min, float max)
        {
            System.Random random = new System.Random();
            double val = (random.NextDouble() * (max - min) + min);
            return (float)val;
        }
        public static Vector3 ToEulerAngles(Quaternion q)
        {
            Vector3 angles = new();

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        public static float ToDegrees(float radians)
        {
            float degrees = (180 / (float)Math.PI) * radians;
            return (degrees);
        }
        public static float ToRadians(float degrees)
        {
            float radians = ((float)Math.PI / 180) * degrees;
            return radians;
        }
        public static float NormalizeAngle(float angle)
        {
            while(angle > 360)
            {
                angle -= 360;
            }
            while(angle < -360)
            {
                angle += 360;
            }
            return angle;
        }
        public static Vector3 AngularVelocityFromQuaternions(Quaternion q1, Quaternion q2, double dt)
        {


            Vector3 v = new Vector3();
            v.X = (float)((2 / dt) * (q1.W * q2.X - q1.X * q2.W - q1.Y * q2.Z + q1.Z * q2.Y));
            v.Y = (float)((2 / dt) * (q1.W * q2.Y + q1.X * q2.Z - q1.Y * q2.W - q1.Z * q2.X));
            v.Z = (float)((2 / dt) * (q1.W * q2.Z - q1.X * q2.Y + q1.Y * q2.X - q1.Z * q2.W));

            return v;

        }
        public static float AngularVelocityFromDegrees(float angle1, float angle0, float dt)
        {
            float angle1Rad = ToRadians(angle1);
            float angle0Rad = ToRadians(angle0);
            return AngularVelocity(angle1Rad, angle0Rad, dt);
        }
        public static float AngularVelocity(float angle1, float angle0, float dt)
        {
            return (angle1 - angle0) / dt;
        }
        public static Vector3 AngularAcceleration(Vector3 w1, Vector3 w0, float dt)
        {
            return (w1 - w0) / dt;
        }
        public static float AngularAcceleration(float w1, float w0, float dt)
        {
            return (w1 - w0) / dt;
        }
        public static void printArray(float[,] array)
        {
            Trace.WriteLine("array");
            for(int i = 0; i < array.GetLength(0); i++)
            {
                string temp = "[";
                for(int j = 0; j < array.GetLength(1); j++)
                {
                    temp += array[i, j] + ", ";
                }
                temp += "]";
                Trace.WriteLine(temp);
            }
        }
        public static bool NearlyEqual(float a, float b, float epsilon = 0.1f)
        {
            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float diff = Math.Abs(a - b);

            if (a == b)
            { // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || absA + absB < float.MinValue)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * float.MaxValue);
            }
            else
            { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }
        public static void callWhenNavigated(Frame frame, Action f)
        {
            if(frame.Content == null)
            {
                frame.Navigated += (sender, args) =>
                {
                    f();
                };
            }
            else
            {
                f();
            }
        }
        public static float randomFloat(float min, float max)
        {
            Random random = new Random();
            float f = random.NextSingle();
            f = f * (max - min);
            return f + min;
        }
        public static Quaternion random_quaternion()
        {
            float x, y, z, u, v, w, s;
            do { x = randomFloat(-1,1); y = randomFloat(-1, 1); z = x * x + y * y; } while (z > 1);
            do { u = randomFloat(-1, 1); v = randomFloat(-1, 1); w = u * u + v * v; } while (w > 1);
            s = (float)Math.Sqrt((1 - z) / w);
            return new Quaternion(x, y, s * u, s * v);
        }
        #region pressure_map
        public static (float, int)[] CountFrequences(Matrix<float> array)
        {
            Dictionary<float, int> elementCounts = new Dictionary<float, int>();


            for (int i = 0; i < array.RowCount; i++)
            {
                for (int j = 0; j < array.ColumnCount; j++)
                {
                    float element = array[i, j];
                    if (elementCounts.ContainsKey(element))
                        elementCounts[element]++;
                    else
                        elementCounts.Add(element, 1);
                }
            }


            (float, int)[] frequences = new (float, int)[elementCounts.Count];
            int k = 0;
            foreach (KeyValuePair<float, int> count in elementCounts)
            {
                frequences[k] = (count.Key, count.Value);
                k++;
            }
            return frequences;
        }
        public static byte ColorToByte(Color color)
        {
            return (byte)((color.R + color.G + color.B) / 3);
        }
        public static int ColorToInt(Color color)
        {
            int colorData = color.R << 16;
            colorData |= color.G << 8;
            colorData |= color.B << 0;
            return colorData;
        }
        // Bitmap se accede [col, row] y Matrix [row, col] habria que cambiar esto
        public static Matrix<float> ImageToMatrix(Bitmap image)
        {
            Matrix<float> floats = Matrix<float>.Build.Dense(image.Width, image.Height);
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    floats[i, j] = ColorToByte(image.GetPixel(i, j));
                }
            }
            return floats;
        }
        public static int SquareDistance(int x1, int y1, int x2, int y2)
        {
            int dist_x = x1 - x2;
            int dist_y = y1 - y2;
            dist_x = dist_x * dist_x;
            dist_y = dist_y * dist_y;
            return dist_x + dist_y;
        }
        public static float SquareDistance(float x1, float y1, float x2, float y2)
        {
            float dist_x = x1 - x2;
            float dist_y = y1 - y2;
            dist_x = dist_x * dist_x;
            dist_y = dist_y * dist_y;
            return dist_x + dist_y;
        }
        public static Tuple<double, double> Average(List<Tuple<int, int>> values)
        {
            double x = 0;
            double y = 0;
            foreach (Tuple<int, int> item in values)
            {
                x += item.Item1;
                y += item.Item2;
            }
            x /= values.Count;
            y /= values.Count;
            return new Tuple<double, double>(x, y);
        }
        public static string GetFilePath(string filename)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string path = Path.Combine(projectDirectory, filename);
            return path;
        }
        #endregion pressure_map
        public static void print(int[] data)
        {
            Trace.Write("[");
            for(int i = 0; i < data.Length; i++)
            {
                Trace.Write(data[i] + ", ");
            }
            Trace.WriteLine("]");
        }
        public static double?[,] replace(double[,] array, double value, double? replacement)
        {
            double?[,] result = new double?[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i, j] == value)
                    {
                        result[i, j] = replacement;
                    }
                    else
                    {
                        result[i, j] = array[i, j];
                    }
                }
            }
            return result;
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
    }
}