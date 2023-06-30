using insoles.Enums;
using insoles.Messages;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Drawing;
using insoles.States;
using System.Diagnostics;
using System.Windows.Documents;
using insoles.Model;

namespace insoles.Services
{
    public class SaveService : ISaveService
    {
        private RegistroState state;
        private DatabaseBridge databaseBridge;
        private VideoWriter[] videoWriters;
        private StringBuilder dataHolder;
        private int frame;
        private float fakets;
        private string fileName;
        private List<string> videoFileNames;
        DateTime testTime;
        private const string header = @"DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT
            TIME	TIME	LARCH	LHALLUX	LHEELR	LHEELL	LMET1	LMET3	LMET5	LTOES	RARCH	RHALLUX	RHEELR	RHEELL	RMET1	RMET3	RMET5	RTOES	RTOTAL	LTOTAL
            FRAME_NUMBERS	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG
            ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL
        ITEM	0	0	x	x	x	x	x	x	x	x	x	x	x	x	x	x	x	x	x	x"
        + "\n";
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
        public SaveService(RegistroState state)
        {
            this.state = state;
        }
        private string FileNameGenerator()
        {
            DateTime now = DateTime.Now;
            testTime = now;
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
        public void AppendCSV(List<Dictionary<Sensor, double>> left, 
            List<Dictionary<Sensor, double>> right, 
            float[] metricLeft, float[] metricRight)
        {
            if (!state.paused)
            {
                if (dataHolder != null)
                {
                    StringBuilder lines = new StringBuilder();
                    for (int i = 0; i < left.Count; i++)
                    {
                        string line = "1 " +
                            fakets.ToString("F2", CultureInfo.InvariantCulture) +
                            " " + frame.ToString() + " " +
                            DictionaryToString(left[i], order) + " " +
                            DictionaryToString(right[i], order) + " " +
                            metricLeft[i].ToString("F2", CultureInfo.InvariantCulture) + " " +
                            metricRight[i].ToString("F2", CultureInfo.InvariantCulture);
                        lines.AppendLine(line);
                        frame++;
                        fakets += 0.01f;
                    }
                    dataHolder.Append(lines);
                }
            }
        }
        private string DictionaryToString(Dictionary<Sensor, double> dict,
            List<Sensor> order)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < order.Count - 1; i++)
            {
                result.Append(dict[order[i]].ToString("F2", CultureInfo.InvariantCulture) + " ");
            }
            result.Append(dict[order[order.Count - 1]].ToString("F2", CultureInfo.InvariantCulture));
            return result.ToString();
        }

        public void AppendVideo(Mat frame, int index)
        {
            if (!state.paused)
            {
                if (videoWriters != null)
                {
                    if (videoWriters[index] != null) // Race condition
                    {
                        lock (videoWriters[index])
                        {
                            if (videoWriters[index] != null)
                                videoWriters[index].Write(frame);
                        }
                    }
                }
            }
        }

        public void Start(ICameraService cameraService)
        {
            videoFileNames = new List<string>();
            string userName = Environment.UserName;
            string path = "C:\\Users\\" + userName + "\\Documents";
            string filePath = path + Path.DirectorySeparatorChar + FileName;
            videoWriters = new VideoWriter[cameraService.NumCamerasOpened];
            for(int i = 0; i < videoWriters.Length; i++)
            {
                string filePath_i = filePath + "cam" + i + ".avi";
                videoFileNames.Add(filePath_i);
                videoWriters[i] = new VideoWriter(filePath_i, 
                    cameraService.getFourcc(i), cameraService.getFps(i), 
                    cameraService.getResolution(i), true);
            }  
            frame = 0;
            fakets = 0;
            dataHolder = new StringBuilder();
            dataHolder.Append(header);
        }

        public Test Stop()
        {
            foreach(VideoWriter videoWriter in videoWriters)
            {
                videoWriter.Dispose();
            }
            videoWriters = null;
            string userName = Environment.UserName;
            string path = "C:\\Users\\" + userName + "\\Documents";
            string filePath = path + Path.DirectorySeparatorChar + FileName + ".txt";
            File.WriteAllTextAsync(filePath, dataHolder.ToString());
            FileName = null;
            Test test = new Test(testTime, filePath, videoFileNames);
            return test;
        }
    }
}
