using insoles.Messages;
using System.Collections.Generic;
using WisewalkSDK;

namespace insoles.Services
{
    public interface IApiService
    {
        void Scan();
        void Connect(List<string> macs);
        void Capture();
        public delegate void InsoleScanEventHandler(List<InsoleScan> data);
        public delegate void InsoleDataEventHandler(List<InsoleData> data);
        public delegate void DeviceEventHandler(Device dev);
        public event InsoleScanEventHandler ScanReceived;
        public event InsoleDataEventHandler DataReceived;
        public event DeviceEventHandler DeviceConnected;
    }
}
