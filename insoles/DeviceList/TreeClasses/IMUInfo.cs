using insoles.Common;
using System.Collections.Generic;

namespace ibcdatacsharp.DeviceList.TreeClasses
{
    // Guarda la información de un IMU
    public class IMUInfo : BaseObject
    {
        private static HashSet<int> idsUsed = new HashSet<int>();
        public int? id
        {
            get { return GetValue<int>("id"); }
            set { SetValue("id", value); }
        }
        public string name
        {
            get { return GetValue<string>("name"); }
            set { SetValue("name", value); }
        }
        public string address
        {
            get { return GetValue<string>("address"); }
            set { SetValue("address", value); }
        }
        public int? battery
        {
            get { return GetValue<int?>("battery"); }
            set { SetValue("battery", value); }
        }
        public bool connected
        {
            get { return GetValue<bool>("connected"); }
            set
            {
                SetValue("connected", value);
                if (!value)
                {
                    if (used)
                    {
                        used = false;
                    }
                }
            }
        }
        public bool used
        {
            get { return GetValue<bool>("used"); }
            set { SetValue("used", value); }
        }
        public string? fw
        {
            get { return GetValue<string>("fw"); }
            set { SetValue("fw", value); }
        }

        public byte? handler { get; set; }

        public void checkJAUpdate()
        {
            NotifyChange("connected");
        }
        public IMUInfo() { }
        public IMUInfo(string name, string address)
        {
            this.name = name;
            this.address = address;
            this.battery = null;
            this.connected = false;
            this.used = false;
            this.fw = null;
        }
        public void setID()
        {
            id = getNextID();
        }
        private static int getNextID()
        {
            //Trace.WriteLine("getNextID");
            //Trace.WriteLine(idsUsed.Count);
            for (int i = 0; i < idsUsed.Count; i++)
            {
                if (!idsUsed.Contains(i))
                {
                    idsUsed.Add(i);
                    return i;
                }
            }
            int id = idsUsed.Count;
            idsUsed.Add(id);
            return id;
        }
        public static void removeIMU(IMUInfo imu)
        {
            //Trace.WriteLine("removeIMU");
            //Trace.WriteLine(imu.id);
            idsUsed.Remove((int)imu.id);
        }
    }
}