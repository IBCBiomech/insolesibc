using System.Collections.Generic;

namespace mvvm.Messages
{
    public class ScanMessage : Message
    {
        public ScanMessage(List<CameraScanMessage> cameras) 
        { 
            this.cameras = cameras;
        }
        public List<CameraScanMessage> cameras {  get; set; }
    }
}
