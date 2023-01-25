using insoles.Common;
using Microsoft.VisualBasic.ApplicationServices;

namespace insoles.DeviceList.TreeClasses
{
    // Guarda la información de una Insole
    public class InsolesInfo : BaseObject
    {
        public int id
        {
            get { return GetValue<int>("id"); }
            set { SetValue("id", value); }
        }
        public byte? handler { get; set; }
        public string name
        {
            get { return GetValue<string>("name"); }
            set { SetValue("name", value); }
        }
        public string side
        {
            get { return GetValue<string>("side"); }
            set { SetValue("side", value); }
        }
        public string address
        {
            get { return GetValue<string>("adress"); }
            set { SetValue("adress", value); }
        }
        public int? battery
        {
            get { return GetValue<int?>("battery"); }
            set { SetValue("battery", value); }
        }
        public bool connected
        {
            get { return GetValue<bool>("connected"); }
            set { SetValue("connected", value); }
        }
        public string? fw
        {
            get { return GetValue<string>("fw"); }
            set { SetValue("fw", value); }
        }
        public InsolesInfo(int id, string name, string side, string address)
        {
            this.id = id;
            this.name = name;
            this.side = side;
            this.address = address;
            this.battery = null;
            this.connected = false;
            this.fw = null;
            this.handler = null;
        }
    }
}
