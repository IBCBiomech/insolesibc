using insoles.Messages;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
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
        private RegistroState state;
        public Wisewalk api { get; private set; }
        private List<Wisewalk.Dev> scanDevices;
        private Dictionary<string, Device> devicesConnected;
        public string? port_selected;
        public string? error;

        public event IApiService.InsoleScanEventHandler ScanReceived;
        public event IApiService.InsoleDataEventHandler DataReceived;
        public event IApiService.MACEventHandler DeviceConnected;
        public event IApiService.MACEventHandler DeviceDisconnected;
        public event IApiService.MACFirmwareBatteryEventHandler HeaderInfoReceived;

        //private static List<Wisewalk.Dev> scanDevices = new List<Wisewalk.Dev>();
        public ApiService(RegistroState state)
        {
            this.state = state;
            api = new Wisewalk();
            api.scanFinished += scanFinishedCallback;
            api.deviceConnected += deviceConnectedCallback;
            api.deviceDisconnected += deviceDisconnectedCallback;
            api.updateDeviceConfiguration += updateDeviceConfigurationCallback;
            api.updateDeviceRTC += updateDeviceRTCCallback;
            api.dataReceived += dataReceivedCallback;
            devicesConnected = new Dictionary<string, Device>();
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
        }
        private Dev findInsole(string mac)
        {
            return scanDevices.FirstOrDefault(de => GetMacAddress(de) == mac);
        }
        private byte findHandler(string mac)
        {
            string handler = devicesConnected.Where(d => d.Value.Id == mac).FirstOrDefault().Key;
            return byte.Parse(handler);
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
        public async void Disconnect(List<string> macs)
        {
            List<int> device_handlers = new List<int>();
            foreach (string mac in macs)
            {
                device_handlers.Add(findHandler(mac));
            }
            if (!api.Disconnect(device_handlers, out error))
            {
                Trace.WriteLine("Disconnect error " + error);
            }
            await Task.Delay(2000);
            foreach (string mac in macs)
            {
                DeviceDisconnected?.Invoke(mac);
            }
        }
        private void deviceConnectedCallback(byte handler, WisewalkSDK.Device dev)
        {
            DeviceConnected?.Invoke(dev.Id);
            devicesConnected[handler.ToString()] = dev;
            api.SetDeviceConfiguration(handler, 100, 3, out error);
        }
        private void deviceDisconnectedCallback(byte handler)
        {
            Device device = devicesConnected[handler.ToString()];
            DeviceDisconnected?.Invoke(device.Id);
            devicesConnected.Remove(handler.ToString());
        }
        private void updateDeviceConfigurationCallback(byte handler, byte sambleRate, byte packetType)
        {
            api.SetRTCDevice(handler, GetDateTime(), out error);
        }
        private DateTime GetDateTime()
        {
            DateTime dateTime = new DateTime(2022, 11, 8, 13, 0, 0, 0);
            return dateTime;
        }
        private void updateDeviceRTCCallback(byte handler, DateTime dateTime)
        {
            Device device = api.GetDevicesConnected()[handler.ToString()];
            HeaderInfoReceived?.Invoke(device.Id, device.HeaderInfo.fwVersion, device.HeaderInfo.battery);
        }
        public void Stop()
        {
            if(!api.StopStream(out error))
            {
                Trace.WriteLine(error);
            }
        }
        public void Pause()
        {
            if (!api.StopStream(out error))
            {
                Trace.WriteLine(error);
            }
        }
        public void Resume()
        {
            if(!api.StartStream(out error))
            {
                Trace.WriteLine(error);
            }
        }
        public string GetMac(byte handler)
        {
            return devicesConnected[handler.ToString()].Id;
        }
    }
}
