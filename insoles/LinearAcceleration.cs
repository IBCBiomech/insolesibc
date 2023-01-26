#define VERSION1 //comentar esto para ejecutar la otra version

using System.IO;
using System.Numerics;
using System.Globalization;
using System;
using System.Diagnostics;

namespace ibcdatacsharp
{
    public struct LinearAcceleration
    {
#if VERSION1
        //Version 1
        private static Vector3 g = new Vector3(0, 0, 9.8f);
        private static Quaternion MultNorm(Quaternion q1, Quaternion q2)
        {
            return Quaternion.Normalize(q1 * q2);
        }
        private static Vector3 quaternionRotateVector(Quaternion q, Vector3 v)
        {
            Quaternion qvq = Quaternion.Conjugate(q) * new Quaternion(v, 0) * q;
            //Quaternion qvq = MultNorm(MultNorm(Quaternion.Conjugate(q), new Quaternion(v, 0)), q);
            return new Vector3(qvq.X, qvq.Y, qvq.Z);
        }
        public static Vector3 calcLinAcc(Quaternion q, Vector3 acc)
        {
            //q = Quaternion.Normalize(q);
            Vector3 gRot = quaternionRotateVector(q, g);
            return gRot - acc;
        }
        public static void test(string filename = "C:\\Temp\\a1.csv")
        {
            using (var reader = new StreamReader(filename))
            {
                int linesToSkip = 1;
                for (int i = 0; i < linesToSkip; i++)
                {
                    reader.ReadLine();
                }
                float maxError = 0;
                float totalError = 0;
                int numLines = 0;
                Vector3 lacc_max_error = Vector3.One;
                Vector3 lacc_cal_max_error = Vector3.One;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    float accx = float.Parse(values[3], CultureInfo.InvariantCulture);
                    float accy = float.Parse(values[4], CultureInfo.InvariantCulture);
                    float accz = float.Parse(values[5], CultureInfo.InvariantCulture);
                    float qx = float.Parse(values[16], CultureInfo.InvariantCulture);
                    float qy = float.Parse(values[17], CultureInfo.InvariantCulture);
                    float qz = float.Parse(values[18], CultureInfo.InvariantCulture);
                    float qw = float.Parse(values[15], CultureInfo.InvariantCulture);
                    float laccx = float.Parse(values[19], CultureInfo.InvariantCulture);
                    float laccy = float.Parse(values[20], CultureInfo.InvariantCulture);
                    float laccz = float.Parse(values[21], CultureInfo.InvariantCulture);
                    Quaternion qsensor = new Quaternion(qx, qy, qz, qw);
                    qsensor = Quaternion.Conjugate(qsensor);
                    Vector3 acc = new Vector3(accx, accy, accz) * 9.8f;
                    Vector3 lacc = new Vector3(laccx, laccy, laccz) * 9.8f;
                    Vector3 lacc_cal = calcLinAcc(qsensor, acc);
                    float diference = Math.Abs(lacc.X - lacc_cal.X) + Math.Abs(lacc.Y - lacc_cal.Y) +
                        Math.Abs(lacc.Z - lacc_cal.Z);
                    float total = Math.Abs(lacc.X + lacc.Y + lacc.Z);
                    float error = diference / total;
                    totalError += error;
                    numLines++;
                    if (error > maxError)
                    {
                        maxError = error;
                        lacc_max_error = lacc;
                        lacc_cal_max_error = lacc_cal;
                    }
                    Trace.WriteLine("teorico " + lacc + " calculado " + lacc_cal);
                }
                Trace.WriteLine("Max error " + (maxError * 100).ToString() + " %");
                float errorMedio = totalError / numLines;
                Trace.WriteLine("Error medio " + (errorMedio * 100).ToString() + " %");
                Trace.WriteLine("Max error " + lacc_max_error + " " + lacc_cal_max_error);
            }
        }
#else
        // Version 2
        private const float G = 9.80665f;
        private static Vector3 g = new Vector3(0, 0, -G);
        private static Quaternion MultNorm(Quaternion q1, Quaternion q2)
        {
            return Quaternion.Normalize(q1 * q2);
        }
        private static Vector3 quaternionRotateVector(Quaternion q, Vector3 v)
        {
            Quaternion qvq = Quaternion.Conjugate(q) * new Quaternion(v, 0) * q;
            //Quaternion qvq = MultNorm(MultNorm(Quaternion.Conjugate(q), new Quaternion(v, 0)), q);
            return new Vector3(qvq.X, qvq.Y, qvq.Z);
        }
        // acc tiene que estar en m/s^2
        public static Vector3 calcLinAcc(Quaternion q, Vector3 acc)
        {
            //q = Quaternion.Normalize(q);
            Vector3 gRot = quaternionRotateVector(q, g);
            Trace.WriteLine("gRot");
            Trace.WriteLine(gRot);
            Vector3 result = gRot - acc;
            //Vector3 result = acc - gRot;
            //result.Z *= -1; //Cambia el signo de la z
            return result;
        }
        public static Vector3 calcLinAcc(Quaternion q, Vector3 acc, bool inputG, bool outputG)
        {
            if (inputG)
            {
                g = new Vector3(0, 0, -1);
            }
            else
            {
                g = new Vector3(0, 0, -G);
            }
            //q = Quaternion.Normalize(q);
            Vector3 gRot = quaternionRotateVector(q, g);
            Trace.WriteLine("gRot");
            Trace.WriteLine(gRot);
            Vector3 result = gRot - acc;
            //Vector3 result = acc - gRot;
            //result.Z *= -1; //Cambia el signo de la z
            if (inputG == outputG) {
                return result;
            }
            if (outputG) //input m/s2 output G
            {
                return result / G;
            }
            else //input G output m/s^2
            {
                return result * G;
            }  
        }
        
        public static void test(string filename = "C:\\Temp\\a1.csv")
        {
            using (var reader = new StreamReader(filename))
            {
                int linesToSkip = 1;
                for (int i = 0; i < linesToSkip; i++)
                {
                    reader.ReadLine();
                }
                float maxError = 0;
                float totalError = 0;
                int numLines = 0;
                Vector3 lacc_max_error = Vector3.One;
                Vector3 lacc_cal_max_error = Vector3.One;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    float accx = float.Parse(values[3], CultureInfo.InvariantCulture);
                    float accy = float.Parse(values[4], CultureInfo.InvariantCulture);
                    float accz = float.Parse(values[5], CultureInfo.InvariantCulture);
                    float qx = float.Parse(values[16], CultureInfo.InvariantCulture);
                    float qy = float.Parse(values[17], CultureInfo.InvariantCulture);
                    float qz = float.Parse(values[18], CultureInfo.InvariantCulture);
                    float qw = float.Parse(values[15], CultureInfo.InvariantCulture);
                    float laccx = float.Parse(values[19], CultureInfo.InvariantCulture);
                    float laccy = float.Parse(values[20], CultureInfo.InvariantCulture);
                    float laccz = float.Parse(values[21], CultureInfo.InvariantCulture);
                    Quaternion qsensor = new Quaternion(qx, qy, qz, qw);
                    qsensor = Quaternion.Conjugate(qsensor);
                    Vector3 acc = new Vector3(accx, accy, accz);
                    Vector3 lacc = new Vector3(laccx, laccy, laccz);
                    Vector3 lacc_cal = calcLinAcc(qsensor, acc, true, true);
                    float diference =  Math.Abs(lacc.X - lacc_cal.X) + Math.Abs(lacc.Y - lacc_cal.Y) +
                        Math.Abs(lacc.Z - lacc_cal.Z);
                    float total = Math.Abs(lacc.X + lacc.Y + lacc.Z);
                    float error = diference / total;
                    totalError += error;
                    numLines++;
                    if(error > maxError)
                    {
                        maxError = error;
                        lacc_max_error = lacc;
                        lacc_cal_max_error = lacc_cal;
                    }
                    Trace.WriteLine("teorico " + lacc + " calculado " + lacc_cal);
                }
                Trace.WriteLine("Max error " + (maxError * 100).ToString() + " %");
                float errorMedio = totalError / numLines;
                Trace.WriteLine("Error medio " + (errorMedio * 100).ToString() + " %");
                Trace.WriteLine("Max error " + lacc_max_error + " " + lacc_cal_max_error);
            }
        }
        public static void testG()
        {
            g = new Vector3(0, 0, -1f);
            Quaternion qsensor = new Quaternion(-0.78f, 0.07f, -0.19f, 0.6f);
            Vector3 acc = new Vector3(1.949f, -9.405f, 2.112f) / G;
            Quaternion qglobal = Quaternion.Conjugate(qsensor);
            Trace.WriteLine("qglobal");
            Trace.WriteLine(qglobal);
            Vector3 linAcc = calcLinAcc(qglobal, acc);
            Trace.WriteLine("linAcc");
            Trace.WriteLine(linAcc.ToString());
            Trace.WriteLine("m/s");
            Trace.WriteLine(linAcc * G);
        }
        public static void testms()
        {
            g = new Vector3(0, 0, -G);
            Quaternion qsensor = new Quaternion(-0.78f, 0.07f, -0.19f, 0.6f);
            Vector3 acc = new Vector3(1.949f, -9.405f, 2.112f);
            Quaternion qglobal = Quaternion.Conjugate(qsensor);
            Trace.WriteLine(qglobal);
            Vector3 linAcc = calcLinAcc(qglobal, acc);
            Trace.WriteLine("linAcc");
            Trace.WriteLine(linAcc.ToString());
            Trace.WriteLine("G");
            Trace.WriteLine(linAcc / G);
        }
        public static void testQuatMult() // Da lo mismo que en python
        {
            Quaternion q1 = new Quaternion(-0.78f, 0.07f, -0.19f, 0.6f);
            Quaternion q2 = new Quaternion(0.4f, 0.07f, -0.18f, 0.2f);
            Quaternion qm = q1 * q2;
            Trace.WriteLine(qm);
        }
#endif
    }
}
