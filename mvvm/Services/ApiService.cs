using CommunityToolkit.Mvvm.Messaging;
using mvvm.Messages;
using mvvm.Models;
using mvvm.Services.Interfaces;
using mvvm.Views.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WisewalkSDK;
using static WisewalkSDK.Protocol_v3;
using static WisewalkSDK.Wisewalk;

namespace mvvm.Services
{
    public class ApiService: IApiService
    {
        public Wisewalk api { get; private set; }
        private List<Wisewalk.Dev> scanDevices;
        public string? port_selected;
        public string? error;

        //private static List<Wisewalk.Dev> scanDevices = new List<Wisewalk.Dev>();
        public ApiService()
        {
            api = new Wisewalk();
            api.scanFinished += scanFinishedCallback;
            api.deviceConnected += deviceConnectedCallback;
            api.dataReceived += dataReceivedCallback;
            WeakReferenceMessenger.Default.Register<ConnectMessage>(this, onConnectMessageReceived);
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
            var scanDevices = devices;
            Trace.WriteLine("# of devices: " + devices.Count);
            ShowScanList(scanDevices);
            List<InsoleScanData> Insoles = new();
            for(int i = 0; i < scanDevices.Count; i++)
            {
                string name = "Wisewalk";
                InsoleScanData insole = new InsoleScanData(name, GetMacAddress(scanDevices[i]));
                Insoles.Add(insole);
            }
            
            ScanMessageInsoles message = new ScanMessageInsoles(Insoles);
            WeakReferenceMessenger.Default.Send(message);
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
            List<InsoleMeasureData> measures = new List<InsoleMeasureData>();
            foreach(var sole in data.Sole)
            {
                measures.Add(new InsoleMeasureData(sole));
            }
            InsoleMeasuresMessage message = new InsoleMeasuresMessage(deviceHandler, measures);
            WeakReferenceMessenger.Default.Send(message);
        }
        public async void Scan()
        {
            Trace.WriteLine("Scan from ApiService");
            await Task.Run(() => ScanDevices());
        }
        public async void Start()
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
        public void onConnectMessageReceived(object sender, ConnectMessage args)
        {
            Trace.WriteLine("onConnectMessageReceived");
            List<Dev> conn_list_dev = new List<Dev>();
            foreach (string mac in args.macs)
            {
                Trace.WriteLine(mac);
                conn_list_dev.Add(findInsole(mac));
            }
            if (!api.Connect(conn_list_dev, out error))
            {
                Trace.WriteLine("Connect error " + error);
            }
        }
        private void deviceConnectedCallback(byte handler, WisewalkSDK.Device dev)
        {
            DeviceConnectedMessage message = new DeviceConnectedMessage(dev);
            WeakReferenceMessenger.Default.Send(message);
        }
    }
}
