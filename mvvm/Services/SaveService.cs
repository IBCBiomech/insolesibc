//#define PRINT_MESSAGES

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using mvvm.Helpers;
using mvvm.Messages;
using mvvm.Services.Interfaces;
using OpenCvSharp;

namespace mvvm.Services
{
    public class SaveService : ISaveService
    {
        private readonly MatType matType = MatType.CV_8UC3;
        private VideoWriter videoWriter;
        private StringBuilder dataHolder;

        private List<Sensor> order = new List<Sensor>() 
        { 
            Sensor.Arch, Sensor.Hallux, Sensor.HeelR, Sensor.HeelL, Sensor.Met1,
            Sensor.Met3, Sensor.Met5, Sensor.Toes
        };
        private int frame;
        private float fakets;
        private string FileName()
        {
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();
            string month = now.Month.ToString().PadLeft(2, '0');
            string day = now.Day.ToString().PadLeft(2, '0');
            string hour = now.Hour.ToString().PadLeft(2, '0');
            string minute = now.Minute.ToString().PadLeft(2, '0');
            string second = now.Second.ToString().PadLeft(2, '0');
            string milisecond = now.Millisecond.ToString().PadLeft(3, '0');
            string filename = year + month + day + '-' + hour + '-' + minute + '-' + second + '-' + milisecond;
            return filename;
        }
        public void Start()
        {
#if PRINT_MESSAGES
            Trace.WriteLine("onRecordMessageReceived from SaveService");
#endif
            string userName = Environment.UserName;
            string path = "C:\\Users\\" + userName + "\\Documents";
            string filePath = path + Path.DirectorySeparatorChar + FileName() + ".avi";
            int fps = 60;
            int Width = 640;
            int Height = 480;
            videoWriter = new VideoWriter(filePath, FourCC.DIVX, fps, new Size(Width, Height));
            frame = 0;
            fakets = 0;
            dataHolder = new StringBuilder();
            WeakReferenceMessenger.Default.Register<StopMessage>(this, 
                onStopMessageReceived);
            WeakReferenceMessenger.Default.Register<FrameAvailableMessage>(this, 
                onFrameAvailableMessageReceived);
            WeakReferenceMessenger.Default.Register<LiveDataCalculationsMessage>(this,
                onLiveCalculationsMessageReceived);
        }
        private async void onStopMessageReceived(object sender, StopMessage args)
        {
#if PRINT_MESSAGES
            Trace.WriteLine("onStopMessageReceived from SaveService");
#endif
            WeakReferenceMessenger.Default.Unregister<FrameAvailableMessage>(this);
            videoWriter.Release();
            videoWriter.Dispose();

            string userName = Environment.UserName;
            string path = "C:\\Users\\" + userName + "\\Documents";
            string filePath = path + Path.DirectorySeparatorChar + FileName() + ".txt";
            await File.WriteAllTextAsync(filePath, dataHolder.ToString());
        }
        private void onFrameAvailableMessageReceived(object sender, FrameAvailableMessage args)
        {
#if PRINT_MESSAGES
            Trace.WriteLine("onFrameAvailableMessageReceived from SaveService");
#endif
            Mat frame = args.frame;
            if (videoWriter != null)
            {
                lock (videoWriter)
                {
                    if (frame.Type() != matType)
                    {
                        frame.ConvertTo(frame, matType);
                    }
                    videoWriter.Write(frame);
                }
            }
        }
        private void onLiveCalculationsMessageReceived(object sender, LiveDataCalculationsMessage args)
        {
#if PRINT_MESSAGES
            Trace.WriteLine("onLiveCalculationsMessageReceived from SaveService");
#endif
            StringBuilder lines = new StringBuilder();
            for(int i = 0; i < args.left.Count; i++)
            {
                string line = "1 " + fakets.ToString("F2") + " " + frame.ToString() + " " +
                args.left[i].ToString(order, args.units) + " " + 
                args.right[i].ToString(order, args.units) + " " + 
                args.leftCalcs[i].ToString() + " " + args.rightCalcs[i].ToString();
                lines.AppendLine(line);
                frame++;
                fakets += 0.01f;
            }
            dataHolder.Append(lines);
        }
    }
}
