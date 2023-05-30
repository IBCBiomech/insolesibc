using insoles.Messages;
using System.Collections.Generic;
using WisewalkSDK;

namespace insoles.Services
{
    public interface IApiService
    {
        void Scan();
        void Connect(List<string> macs);
        void ConnectAll();
        void Capture();
        public delegate void InsoleScanEventHandler(List<InsoleScan> data);
        public delegate void InsoleDataEventHandler(byte handler, List<InsoleData> data);
        public delegate void DeviceEventHandler(Device dev);
        public event InsoleScanEventHandler ScanReceived;
        public event InsoleDataEventHandler DataReceived;
        public event DeviceEventHandler DeviceConnected;
    }
}
