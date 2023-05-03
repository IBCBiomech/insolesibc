using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        public void SaveFrame(Mat frame)
        {
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
            Trace.WriteLine("onRecordMessageReceived from SaveService");
            string userName = Environment.UserName;
            string path = "C:\\Users\\" + userName + "\\Documents";
            string filePath = path + Path.DirectorySeparatorChar + FileName() + ".avi";
            int fps = 25;
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
        private void onStopMessageReceived(object sender, StopMessage args)
        {
            Trace.WriteLine("onStopMessageReceived from SaveService");
            WeakReferenceMessenger.Default.Unregister<FrameAvailableMessage>(this);
            videoWriter.Release();
            videoWriter.Dispose();
        }
        private void onFrameAvailableMessageReceived(object sender, FrameAvailableMessage args)
        {
            //Trace.WriteLine("onFrameAvailableMessageReceived from SaveService");
            LockedItem<Mat> lockedFrame = args.lockedFrame;
            if (videoWriter != null)
            {
                lock (videoWriter)
                {
                    if (lockedFrame.Item.Type() != matType)
                    {
                        lockedFrame.Item.ConvertTo(lockedFrame.Item, matType);
                    }
                    videoWriter.Write(lockedFrame.Item);
                }
            }
        }
        private void onLiveCalculationsMessageReceived(object sender, LiveDataCalculationsMessage args)
        {
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
