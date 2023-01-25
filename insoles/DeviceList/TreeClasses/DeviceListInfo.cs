using insoles.Common;
using System.Collections.ObjectModel;

namespace insoles.DeviceList.TreeClasses
{
    // Guarda una lista de IMUs una lista de Camaras y una lista de Insoles
    public class DeviceListInfo: BaseObject
    {
        public ObservableCollection<CameraInfo> cameras
        {
            get { return GetValue<ObservableCollection<CameraInfo>>("cameras"); }
            set { SetValue("cameras", value); }
        }
        public ObservableCollection<InsolesInfo> insoles
        {
            get { return GetValue<ObservableCollection<InsolesInfo>>("insoles"); }
            set { SetValue("insoles", value); }
        }
        public DeviceListInfo()
        {
            cameras = new ObservableCollection<CameraInfo>();
            insoles = new ObservableCollection<InsolesInfo>();
        }
    }
}
