using CommunityToolkit.Mvvm.Messaging;
using mvvm.Messages;
using mvvm.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static WisewalkSDK.Wisewalk;
using Wpf.Ui.Dpi;

namespace mvvm.Services
{
    class FakeApiService : IApiService
    {
        private List<InsoleScanData> Insoles;
        public FakeApiService()
        {
            WeakReferenceMessenger.Default.Register<ConnectMessage>(this, onConnectMessageReceived);
        }
        public void Scan()
        {
            Trace.WriteLine("Scan from FakeApiService");
            Insoles = new()
            {
                new InsoleScanData("Wisewalk", "AC:DE:FG"),
                new InsoleScanData("Wisewalk", "BA:DE:FG")
            };
            ScanMessageInsoles message = new ScanMessageInsoles(Insoles);
            WeakReferenceMessenger.Default.Send(message);
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
        public void onConnectMessageReceived(object sender, ConnectMessage args)
        {
            Trace.WriteLine("onConnectMessageReceived");
            foreach (string mac in args.macs)
            {
                WisewalkSDK.Device dev = new();
                dev.Id = mac;
                DeviceConnectedMessage message = new DeviceConnectedMessage(dev);
                WeakReferenceMessenger.Default.Send(message);
            }
        }
    }
}
