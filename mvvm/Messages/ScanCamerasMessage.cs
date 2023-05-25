﻿using System.Collections.Generic;

namespace mvvm.Messages
{
    public class ScanCamerasMessage : Message
    {
        public ScanCamerasMessage(List<CameraScan> cameras) 
        { 
            this.cameras = cameras;
        }
        public List<CameraScan> cameras {  get; set; }
    }
}
