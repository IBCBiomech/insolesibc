using insoles.Common;

namespace insoles.DeviceList.TreeClasses
{
    // Guarda la información de una Insole
    public class InsolesInfo : BaseObject
    {
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
        public InsolesInfo(string name, string side)
        {
            this.name = name;
            this.side = side;
        }
    }
}
