using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using insolesMVVM.Messages;

namespace insolesMVVM.Models
{
    [ObservableObject]
    public partial class Camera
    {
        [ObservableProperty]
        private bool isSelected;
        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private int number;
        [ObservableProperty]
        private int? fps;
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
