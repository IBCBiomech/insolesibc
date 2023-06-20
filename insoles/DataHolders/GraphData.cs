using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.DataHolders
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
        public double time(int frame)
        {
            try
            {
                int index = frame - minFrame;
                return frames[index].time;
            }
            catch (Exception e)
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
}
