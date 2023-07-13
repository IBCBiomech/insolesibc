﻿using insoles.Messages;
using insoles.States;
using System.Collections.Generic;
using WisewalkSDK;

namespace insoles.Services
{
    public interface IApiService
    {
        void Scan();
        void Connect(List<string> macs);
        void Disconnect(List<string> macs);
        void Capture();
        void Stop();
        void Pause();
        void Resume();
        float Latency();
        string GetMac(byte handler);
        public delegate void InsoleScanEventHandler(List<InsoleScan> data);
        public delegate void InsoleDataEventHandler(byte handler, List<InsoleData> data);
        public delegate void MACEventHandler(string mac);
        public delegate void MACFirmwareBatteryEventHandler(string mac, string firmware, int battery);
        public event InsoleScanEventHandler ScanReceived;
        public event InsoleDataEventHandler DataReceived;
        public event MACEventHandler DeviceConnected;
        public event MACEventHandler DeviceDisconnected;
        public event MACFirmwareBatteryEventHandler HeaderInfoReceived;
    }
}
