using CommunityToolkit.Mvvm.Messaging;
using mvvm.Messages;
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

namespace mvvm.Services
{
    public class ApiService: IApiService
    {
        public Wisewalk api { get; private set; }
        public string? port_selected;
        public string? error;

        //private static List<Wisewalk.Dev> scanDevices = new List<Wisewalk.Dev>();
        public ApiService()
        {
            api = new Wisewalk();
            api.scanFinished += Api_scanFinished;
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
        private void Api_scanFinished(List<Wisewalk.Dev> devices)
        {
            var scanDevices = devices;
            Trace.WriteLine("# of devices: " + devices.Count);
            ShowScanList(scanDevices);
            List<InsoleScanData> IMUs = new();
            for(int i = 0; i < scanDevices.Count; i++)
            {
                string name = "Wisewalk";
                InsoleScanData imu = new InsoleScanData(name, GetMacAddress(scanDevices[i]));
                IMUs.Add(imu);
            }
            IMUs.Add(new InsoleScanData("Wisewalk", "AC:DE:FG"));
            IMUs.Add(new InsoleScanData("Wisewalk", "BA:DE:FG"));
            ScanMessageInsoles message = new ScanMessageInsoles(IMUs);
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
        public async void Scan()
        {
            Trace.WriteLine("Scan from ApiService");
            await Task.Run(() => ScanDevices());
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }
    }
}
