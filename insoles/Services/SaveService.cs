﻿using insoles.Enums;
using insoles.Messages;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace insoles.Services
{
    public class SaveService : ISaveService
    {
        private readonly MatType matType = MatType.CV_8UC3;
        private VideoWriter videoWriter;
        private StringBuilder dataHolder;
        private int frame;
        private float fakets;
        private string fileName;
        private string FileName
        {
            get
            {
                if(fileName == null)
                {
                    fileName = FileNameGenerator();
                }
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }
        private List<Sensor> order = new List<Sensor>()
        {
            Sensor.Arch, Sensor.Hallux, Sensor.HeelR, Sensor.HeelL, Sensor.Met1,
            Sensor.Met3, Sensor.Met5, Sensor.Toes
        };
        private string FileNameGenerator()
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
        public void AppendCSV(List<InsoleData> left, List<InsoleData> right, float[] metricLeft, float[] metricRight)
        {
            if (dataHolder != null)
            {
                StringBuilder lines = new StringBuilder();
                for (int i = 0; i < left.Count; i++)
                {
                    string line = "1 " + fakets.ToString("F2") + " " + frame.ToString() + " " +
                    left[i].ToString(order) + " " +
                    right[i].ToString(order) + " " +
                    metricLeft[i].ToString() + " " + metricRight[i].ToString();
                    lines.AppendLine(line);
                    frame++;
                    fakets += 0.01f;
                }
                dataHolder.Append(lines);
            }
        }

        public void AppendVideo(Mat frame)
        {
            if (videoWriter != null)
            {
                lock (videoWriter)
                {
                    if (frame.Type() != matType)
                    {
                        frame.ConvertTo(frame, matType);
                    }
                    if(videoWriter != null)
                        videoWriter.Write(frame);
                }
            }
        }

        public void Start(int fps, Size size)
        {
            string userName = Environment.UserName;
            string path = "C:\\Users\\" + userName + "\\Documents";
            string filePath = path + Path.DirectorySeparatorChar + FileName + ".avi";
            videoWriter = new VideoWriter(filePath, FourCC.DIVX, fps, size);
            frame = 0;
            fakets = 0;
            dataHolder = new StringBuilder();
        }

        public async void Stop()
        {
            videoWriter.Release();
            videoWriter.Dispose();
            videoWriter = null;

            string userName = Environment.UserName;
            string path = "C:\\Users\\" + userName + "\\Documents";
            string filePath = path + Path.DirectorySeparatorChar + FileName + ".txt";
            await File.WriteAllTextAsync(filePath, dataHolder.ToString());
            FileName = null;
        }
    }
}