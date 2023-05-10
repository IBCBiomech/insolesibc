using CommunityToolkit.Mvvm.Messaging;
using insolesMVVM.Messages;
using insolesMVVM.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace insolesMVVM.Services
{
    public class FakeApiService : IApiService
    {
        private List<InsoleScan> Insoles;
        private List<InsoleScan> ConnectedInsoles = new();
        private Timer timer; 
        public FakeApiService()
        {
            WeakReferenceMessenger.Default.Register<ScanMessage>(this, onScanMessageReceived);
            WeakReferenceMessenger.Default.Register<ConnectInsolesMessage>(this, onConnectMessageReceived);
            WeakReferenceMessenger.Default.Register<CaptureMessage>(this, onCaptureMessageReceived);
        }
        public void onScanMessageReceived(object sender, ScanMessage triggerMessage)
        {
            Trace.WriteLine("Scan from FakeApiService");
            Insoles = new()
            {
                new InsoleScan("Wisewalk", "AC:DE:FG"),
                new InsoleScan("Wisewalk", "BA:DE:FG")
            };
            ScanInsolesMessage message = new ScanInsolesMessage(Insoles);
            WeakReferenceMessenger.Default.Send(message);
        }

        public void onCaptureMessageReceived(object sender, CaptureMessage message)
        {
            for (int i = 0; i < ConnectedInsoles.Count; i++)
            {
                int index = i;
                timer = new Timer();
                timer.Interval = 40;
                timer.Elapsed += (s, e) => GenerateData(index);
                timer.Start();
            }
        }
        public void onConnectMessageReceived(object sender, ConnectInsolesMessage args)
        {
            Trace.WriteLine("onConnectMessageReceived");
            foreach (string mac in args.macs)
            {
                WisewalkSDK.Device dev = new();
                dev.Id = mac;
                ConnectedInsoles.Add(Insoles.Where((insole) => insole.address == mac).First());
                DeviceConnectedMessage message = new DeviceConnectedMessage(dev);
                WeakReferenceMessenger.Default.Send(message);
            }
        }
        private void GenerateData(int handler)
        {
            List<InsoleData> measures = new List<InsoleData>();
            for(int i = 0; i < 4; i++)
            {
                Random random = new Random();
                measures.Add(new InsoleData(random));
            }
            InsoleMeasuresMessage message = new InsoleMeasuresMessage((byte)handler, measures);
            WeakReferenceMessenger.Default.Send(message);
        }
    }
}
