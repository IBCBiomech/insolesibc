using mvvm.Helpers;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class FrameAvailableMessage : Message
    {
        public FrameAvailableMessage(LockedItem<Mat> lockedFrame) 
        {
            this.lockedFrame = lockedFrame;
        }
        public LockedItem<Mat> lockedFrame { get; private set; }
    }
}
