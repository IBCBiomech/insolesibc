using insoles.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace insoles.Services
{
    public class FakeApiService : IApiService
    {
        private List<InsoleScan> Insoles;
        private List<InsoleScan> ConnectedInsoles = new();
        private Timer timer;
        public bool capturing { get; private set; } = false;

        public event IApiService.InsoleScanEventHandler ScanReceived;
        public event IApiService.InsoleDataEventHandler DataReceived;
        public event IApiService.DeviceEventHandler DeviceConnected;

        public FakeApiService()
        {
            
        }
        public void Scan()
        {
            Trace.WriteLine("Scan from FakeApiService");
            Insoles = new()
            {
                new InsoleScan("Wisewalk", "AC:DE:FG"),
                new InsoleScan("Wisewalk", "BA:DE:FG")
            };
            ScanReceived?.Invoke(Insoles);
            //ConnectedInsoles = Insoles;
        }

        public void Capture()
        {
            for (int i = 0; i < ConnectedInsoles.Count; i++)
            {
                int index = i;
                timer = new Timer();
                timer.Interval = 40;
                timer.Elapsed += (s, e) => GenerateData(index);
                timer.Start();
            }
            capturing = true;
        }
        public void Connect(List<string> macs)
        {
            Trace.WriteLine("onConnectMessageReceived");
            foreach (string mac in macs)
            {
                WisewalkSDK.Device dev = new();
                dev.Id = mac;
                ConnectedInsoles.Add(Insoles.Where((insole) => insole.MAC == mac).First());
                DeviceConnected?.Invoke(dev);
            }
        }
        public void ConnectAll()
        {
            Trace.WriteLine("onConnectAllMessageReceived");
            foreach (var insole in Insoles)
            {
                WisewalkSDK.Device dev = new();
                dev.Id = insole.MAC;
                ConnectedInsoles.Add(insole);
                DeviceConnected?.Invoke(dev);
            }
        }
        private void GenerateData(int handler)
        {
            List<InsoleData> measures = new List<InsoleData>();
            for (int i = 0; i < 4; i++)
            {
                Random random = new Random();
                measures.Add(new InsoleData(random));
            }
            DataReceived?.Invoke((byte)handler, measures);
        }
    }
}
