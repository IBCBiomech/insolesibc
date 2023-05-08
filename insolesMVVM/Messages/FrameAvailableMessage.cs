using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insolesMVVM.Messages
{
    public class FrameAvailableMessage : Message
    {
        public int index { get; set; }
        public Mat frame {  get; set; }
        public FrameAvailableMessage(int index, Mat frame) 
        {
            this.frame = frame;
            this.index = index;
        }
    }
}
