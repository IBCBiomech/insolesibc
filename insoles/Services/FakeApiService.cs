using insoles.Messages;
using insoles.States;
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
        private RegistroState state;
        private List<InsoleScan> Insoles;
        private List<InsoleScan> ConnectedInsoles = new();
        private List<Timer> timers;

        public event IApiService.InsoleScanEventHandler ScanReceived;
        public event IApiService.InsoleDataEventHandler DataReceived;
        public event IApiService.MACEventHandler DeviceConnected;
        public event IApiService.MACEventHandler DeviceDisconnected;
        public event IApiService.MACFirmwareBatteryEventHandler HeaderInfoReceived;

        public FakeApiService(RegistroState state)
        {
            this.state = state;
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
            timers = new List<Timer>();
            for (int i = 0; i < ConnectedInsoles.Count; i++)
            {
                int index = i;
                Timer timer = new Timer();
                timer.Interval = 40;
                timer.Elapsed += (s, e) => GenerateData(index);
                timer.Start();
                timers.Add(timer);
            }
        }
        public void Connect(List<string> macs)
        {
            Trace.WriteLine("onConnectMessageReceived");
            foreach (string mac in macs)
            {
                ConnectedInsoles.Add(Insoles.Where((insole) => insole.MAC == mac).First());
                DeviceConnected?.Invoke(mac);
                HeaderInfoReceived?.Invoke(mac, "1.10", 100);
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

        public void Disconnect(List<string> macs)
        {
            foreach(string mac in macs)
            {
                ConnectedInsoles.Remove(Insoles.Where((insole) => insole.MAC == mac).First());
                DeviceDisconnected?.Invoke(mac);
            }
        }

        public void Stop()
        {
            foreach(Timer timer in timers)
            {
                timer.Stop();
            }
        }
        public void Pause()
        {
            foreach (Timer timer in timers)
            {
                timer.Stop();
            }
        }
        public void Resume()
        {
            foreach (Timer timer in timers)
            {
                timer.Start();
            }
        }
    }
}
