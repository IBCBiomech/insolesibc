using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insolesMVVM.Messages
{
    public class DeviceConnectedMessage : Message
    {
        public WisewalkSDK.Device device { get; set; }
        public DeviceConnectedMessage(WisewalkSDK.Device device)
        {
            this.device = device;
        }
    }
}
