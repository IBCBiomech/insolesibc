﻿//#define SWAP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace insoles.Graphs
{
    public class GraphData
    {
        public FrameData[] frames;
        public FrameData this[int index]
        {
            get { return frames[index]; }
        }
        public int length
        {
            get { return frames.Length; }
        }
        public int minFrame
        {
            get { return frames[0].frame; }
        }
        public int maxFrame
        {
            get { return frames[frames.Length - 1].frame; }
        }
        public double minTime
        {
            get { return frames[0].time; }
        }
        public double maxTime
        {
            get { return frames[frames.Length - 1].time; }
        }
        public double time(int frame){
            try
            {
                int index = frame - minFrame;
                return frames[index].time;
            }
            catch(Exception e)
            {
                Trace.WriteLine(minFrame);
                throw e;
            }
        }
        public GraphData(FrameData[] frames)
        {
            this.frames = frames;
        }
    }
    #region Factories
    public class FrameDataMetaFactory
    {
        public List<FrameDataFactory> factories;
        private FrameDataFactory current;
        public FrameDataMetaFactory()
        {
            factories = new List<FrameDataFactory>();
            factories.Add(new FrameDataFactoryInsoles());
        }
        public void changeHeader(string header)
        {
            int minSimilarity = int.MaxValue;
            foreach(FrameDataFactory factory in factories)
            {
                int similarity = factory.compareSimilarity(header);
                if(similarity < minSimilarity)
                {
                    minSimilarity = similarity;
                    current = factory;
                }
            }
            //current = factories.Aggregate((curMin, x) => curMin == null || (x.similarity ?? int.MaxValue) < x.similarity ? x : curMin);
        }
        public void addLine(string header)
        {
            current.addLine(header);
        }
        public GraphData getData()
        {
            return current.getData();
        }
    }
    public abstract class FrameDataFactory
    {
        protected virtual string header { get; }
        public abstract void addLine(string line);
        public abstract GraphData getData();
        public int compareSimilarity(string header)
        {
            int value1 = 0;
            foreach (char c in this.header)
            {
                int tmp = c;
                value1 += c;
            }
            int value2 = 0;
            foreach (char c in header)
            {
                int tmp = c;
                value2 += c;
            }
            int similarity = Math.Abs(value1 - value2);
            return similarity;
        }
    }
    public class FrameDataFactoryInsoles : FrameDataFactory
    {
        protected override string header { get { return Config.csvHeaderInsoles; } }
        private List<FrameDataInsoles> data;
        public FrameDataFactoryInsoles()
        {
            data = new List<FrameDataInsoles>();
        }
        public override void addLine(string line)
        {
            data.Add(new FrameDataInsoles(line));
        }
        public override GraphData getData()
        {
            return new GraphData(data.ToArray());
        }
    }
    #endregion Factories
    public abstract class FrameData
    {
        public double time { get; set; }
        public int frame { get; set; }
        protected double parseDouble(string s)
        {
            string s_point = s.Replace(",", ".");
            double result = double.Parse(s_point, CultureInfo.InvariantCulture);
            return result;
        }
        private static float parseFloat(string s)
        {
            string s_point = s.Replace(",", ".");
            float result = float.Parse(s_point, CultureInfo.InvariantCulture);
            return result;
        }
        public static float getFloat(string[] values, int index)
        {
            if(index < values.Length)
            {
                return parseFloat(values[index]);
            }
            else
            {
                return 0f;
            }
        }
        protected double getDouble(string[] values, int index)
        {
            if (index < values.Length)
            {
                return parseDouble(values[index]);
            }
            else
            {
                return 0.0;
            }
        }
        protected int getInt(string[] values, int index)
        {
            if (index < values.Length)
            {
                return int.Parse(values[index]);
            }
            else
            {
                return 0;
            }
        }
    }
    public class FrameDataInsoles: FrameData
    {
        public DataInsole left { get; set; }
        public DataInsole right { get; set; }
        public FrameDataInsoles(string csvLine)
        {
            string[] values = System.Text.RegularExpressions.Regex.Split(csvLine, @"\s+");
            time = getDouble(values, 1);
            frame = getInt(values, 2);
            Func<float, float> transformFunc = GraphsConfig.transformFunc;
#if SWAP
            right = new DataInsole(values, 3, transformFunc);
            left = new DataInsole(values, 11, transformFunc);
#else
            left = new DataInsole(values, 3, transformFunc);
            right = new DataInsole(values, 11, transformFunc);
#endif
        }
    }
    public class DataInsole
    {
        public Dictionary<Sensor, int> pressures { get; private set; } = new Dictionary<Sensor, int>();
        public int getPressure(Sensor sensor)
        {
            return pressures[sensor];
        }
        public int this[Sensor sensor]
        {
            get { return pressures[sensor]; }
            set { pressures[sensor] = value; }
        }
        public DataInsole(string[] values, int firstIndex)
        {
            pressures[Sensor.ARCH] = int.Parse(values[firstIndex]);
            pressures[Sensor.HALLUX] = int.Parse(values[firstIndex + 1]);
            pressures[Sensor.HEEL_L] = int.Parse(values[firstIndex + 2]);
            pressures[Sensor.HEEL_R] = int.Parse(values[firstIndex + 3]);
            pressures[Sensor.MET1] = int.Parse(values[firstIndex + 4]);
            pressures[Sensor.MET3] = int.Parse(values[firstIndex + 5]);
            pressures[Sensor.MET5] = int.Parse(values[firstIndex + 6]);
            pressures[Sensor.TOES] = int.Parse(values[firstIndex + 7]);
        }
        public DataInsole(string[] values, int firstIndex, Func<float, float> transformFunc)
        {
            pressures[Sensor.ARCH] = (int)transformFunc(FrameData.getFloat(values, firstIndex));
            pressures[Sensor.HALLUX] = (int)transformFunc(FrameData.getFloat(values, firstIndex + 1));
            pressures[Sensor.HEEL_L] = (int)transformFunc(FrameData.getFloat(values, firstIndex + 2));
            pressures[Sensor.HEEL_R] = (int)transformFunc(FrameData.getFloat(values, firstIndex + 3));
            pressures[Sensor.MET1] = (int)transformFunc(FrameData.getFloat(values, firstIndex + 4));
            pressures[Sensor.MET3] = (int)transformFunc(FrameData.getFloat(values, firstIndex + 5));
            pressures[Sensor.MET5] = (int)transformFunc(FrameData.getFloat(values, firstIndex + 6));
            pressures[Sensor.TOES] = (int)transformFunc(FrameData.getFloat(values, firstIndex + 7));
        }
        public DataInsole()
        {
            pressures[Sensor.ARCH] = 0;
            pressures[Sensor.HALLUX] = 0;
            pressures[Sensor.HEEL_L] = 0;
            pressures[Sensor.HEEL_R] = 0;
            pressures[Sensor.MET1] = 0;
            pressures[Sensor.MET3] = 0;
            pressures[Sensor.MET5] = 0;
            pressures[Sensor.TOES] = 0;
        }
        public int totalPressure
        {
            get
            {
                int result = 0;
                foreach(Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    result += pressures[sensor];
                }
                return result;
            }
        }
        public override string ToString() {
            string result = "";
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                result += sensor.ToString() + " " + pressures[sensor] + ", ";
            }
            return result;
        }
    }
}
