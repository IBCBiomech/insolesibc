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
        public List<int> fpsAvailable
        {
            get;
            set;
        }
        private int _fps;
        public int fps
        {
            get { return _fps; }
            set
            {
                _fps = value;
                OnPropertyChanged();
            }
        }
        public CameraModel(int number, string name, List<int> fpsAvailable, RegistroVM VM)
        {
            this.number = number;
            this.name = name;
            this.VM = VM;
            this.fpsAvailable = fpsAvailable;
            this.fps = fpsAvailable[0];
        }
    }
}
