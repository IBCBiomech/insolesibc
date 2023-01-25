using System.Windows.Media;
using System.Windows;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Windows.Navigation;

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
    }
}