using insolesMVVM.Messages;
using ReactiveUI;

namespace insolesMVVM.Models
{
    public partial class Camera : ReactiveObject
    {
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => this.RaiseAndSetIfChanged(ref isSelected, value);
        }
        private string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
        private int number;
        public int Number
        {
            get => number;
            set => this.RaiseAndSetIfChanged(ref number, value);
        }
        private int? fps;
        public int? Fps
        {
            get => fps;
            set => this.RaiseAndSetIfChanged(ref fps, value);
        }
        public Camera(int number, string name)
        {
            Number = number;
            Name = name;
            Fps = null;
            IsSelected = false;
        }
        public Camera(CameraScan camera)
        {
            Number = camera.number; 
            Name = camera.name;
            Fps = null;
            IsSelected = false;
        }
    }
}
