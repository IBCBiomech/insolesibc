using insoles.Messages;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WisewalkSDK;
using static WisewalkSDK.Wisewalk;

namespace insoles.Services
{
    public class ApiService : IApiService
    {
        public Wisewalk api { get; private set; }
        private List<Wisewalk.Dev> scanDevices;
        public string? port_selected;
        public string? error;
        public bool capturing { get; private set; } = false;

        public event IApiService.InsoleScanEventHandler ScanReceived;
        public event IApiService.InsoleDataEventHandler DataReceived;
        public event IApiService.DeviceEventHandler DeviceConnected;

        //private static List<Wisewalk.Dev> scanDevices = new List<Wisewalk.Dev>();
        public ApiService()
        {
            api = new Wisewalk();
            api.scanFinished += scanFinishedCallback;
            api.deviceConnected += deviceConnectedCallback;
            api.dataReceived += dataReceivedCallback;
        }
        public void ShowPorts()
        {
            var ports = api.GetUsbDongles();
            foreach (Wisewalk.ComPort port in ports)
            {
                Match match1 = Regex.Match(port.description, "nRF52 USB CDC BLE*", RegexOptions.IgnoreCase);
                if (match1.Success)
                {
                    port_selected = port.name;
                    Trace.WriteLine(port.description);
                }
            }
        }
        public void ScanDevices()
        {
            ShowPorts();
            api.Open(port_selected, out error);
            if (!api.ScanDevices(out error))
            {
                // Error
                Trace.WriteLine("", "Error to scan devices - " + error);
            }
            else
            {
                Thread.Sleep(2000);
            }
        }
        private void scanFinishedCallback(List<Wisewalk.Dev> devices)
        {
            scanDevices = devices;
            Trace.WriteLine("# of devices: " + devices.Count);
            ShowScanList(scanDevices);
            List<InsoleScan> Insoles = new();
            for (int i = 0; i < scanDevices.Count; i++)
            {
                string name = "Wisewalk";
                InsoleScan insole = new InsoleScan(name, GetMacAddress(scanDevices[i]));
                Insoles.Add(insole);
            }
            ScanReceived?.Invoke(Insoles);
        }
        private void ShowScanList(List<Wisewalk.Dev> devices)
        {
            for (int idx = 0; idx < devices.Count; idx++)
            {
                string macAddress = devices[idx].mac[5].ToString("X2") + ":" + devices[idx].mac[4].ToString("X2") + ":" + devices[idx].mac[3].ToString("X2") + ":" +
                                    devices[idx].mac[2].ToString("X2") + ":" + devices[idx].mac[1].ToString("X2") + ":" + devices[idx].mac[0].ToString("X2");


                Trace.WriteLine("MacAddress: ", " * " + macAddress);
            }
        }
        private string GetMacAddress(Wisewalk.Dev device)
        {
            string mac = "";

            mac = device.mac[5].ToString("X2") + ":" + device.mac[4].ToString("X2") + ":" + device.mac[3].ToString("X2") + ":" +
                                    device.mac[2].ToString("X2") + ":" + device.mac[1].ToString("X2") + ":" + device.mac[0].ToString("X2");

            return mac;
        }

        private void dataReceivedCallback(byte deviceHandler, WisewalkSDK.WisewalkData data)
        {
            List<InsoleData> measures = new List<InsoleData>();
            foreach (var sole in data.Sole)
            {
                measures.Add(new InsoleData(sole));
            }
            DataReceived?.Invoke(deviceHandler, measures);
        }

        public async void Scan()
        {
            Trace.WriteLine("Scan from ApiService");
            await Task.Run(() => ScanDevices());
        }
        public async void Capture()
        {
            Trace.WriteLine("Start from ApiService");
            api.SetDeviceConfiguration(0, 100, 3, out error);
            api.SetDeviceConfiguration(1, 100, 3, out error);
            await Task.Delay(2000);
            api.StartStream(out error);
            capturing = true;
        }
        private Dev findInsole(string mac)
        {
            return scanDevices.FirstOrDefault(de => GetMacAddress(de) == mac);
        }
        public void Connect(List<string> macs)
        {
            Trace.WriteLine("onConnectMessageReceived");
            List<Dev> conn_list_dev = new List<Dev>();
            foreach (string mac in macs)
            {
                Trace.WriteLine(mac);
                conn_list_dev.Add(findInsole(mac));
            }
            if (!api.Connect(conn_list_dev, out error))
            {
                Trace.WriteLine("Connect error " + error);
            }
        }
        public void ConnectAll()
        {
            Trace.WriteLine("onConnectMessageReceived connecting all scanned");
            if (!api.Connect(scanDevices, out error))
            {
                Trace.WriteLine("Connect error " + error);
            }
        }
        private void deviceConnectedCallback(byte handler, WisewalkSDK.Device dev)
        {
            DeviceConnected?.Invoke(dev);
        }
    }
}
