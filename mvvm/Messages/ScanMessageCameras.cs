using System.Collections.Generic;

namespace mvvm.Messages
{
    public class ScanMessageCameras : Message
    {
        public ScanMessageCameras(List<CameraScanData> cameras) 
        { 
            this.cameras = cameras;
        }
        public List<CameraScanData> cameras {  get; set; }
    }
}
