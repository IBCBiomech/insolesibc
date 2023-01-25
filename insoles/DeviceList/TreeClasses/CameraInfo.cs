using insoles.Common;


namespace insoles.DeviceList.TreeClasses
{
    // Guarda la información de una camara
    public class CameraInfo : BaseObject
    {
        public string name
        {
            get { return GetValue<string>("name"); }
            set { SetValue("name", value); }
        }
        public int number
        {
            get { return GetValue<int>("number"); }
            set { SetValue("number", value); }
        }
        public int? fps
        {
            get { return GetValue<int?>("fps"); }
            set { SetValue("fps", value); }
        }
        public CameraInfo(int number, string name)
        {
            this.number = number;
            this.name = name;
            this.fps = null;
        }
    }
}
