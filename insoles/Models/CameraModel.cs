using insoles.Utilities;
using insoles.ViewModel;
using System.Collections.Generic;

namespace insoles.Model
{
    public class CameraModel : ModelBase
    {
        public RegistroVM VM { get; set; }
        public string name { get; set; }
        public int number { get; set; }
        public List<int> fpsAvailable {get;set;}
        public Dictionary<int, List<System.Drawing.Size>> fpsResolutions { get; set; }
        private List<System.Drawing.Size> _resolutionsAvailable;
        public List<System.Drawing.Size> resolutionsAvailable
        {
            get { return _resolutionsAvailable; }
            set 
            { 
                _resolutionsAvailable = value;
                OnPropertyChanged();
            }
        }
        private System.Drawing.Size _resolution;
        public System.Drawing.Size resolution
        {
            get { return _resolution; }
            set {
                _resolution = value;
                OnPropertyChanged();
            }
        }
        private int _fps;
        public int fps
        {
            get { return _fps; }
            set
            {
                _fps = value;
                resolutionsAvailable = fpsResolutions[value];
                resolution = resolutionsAvailable[0];
                OnPropertyChanged();
            }
        }
        public CameraModel(int number, string name, List<int> fpsAvailable, Dictionary<int, List<System.Drawing.Size>> fpsResolutions, RegistroVM VM)
        {
            this.number = number;
            this.name = name;
            this.VM = VM;
            this.fpsAvailable = fpsAvailable;
            this.fpsResolutions = fpsResolutions;

            this.fps = fpsAvailable[0];
            this.resolutionsAvailable = fpsResolutions[fps];
            this.resolution = resolutionsAvailable[0];
        }
    }
}
