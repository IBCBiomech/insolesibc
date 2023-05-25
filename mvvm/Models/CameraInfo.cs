using CommunityToolkit.Mvvm.ComponentModel;
using mvvm.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Models
{
    [ObservableObject]
    public partial class CameraInfo
    {
        [ObservableProperty]
        private bool isSelected;
        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private int number;
        [ObservableProperty]
        private int? fps;
        public CameraInfo(int number, string name)
        {
            Number = number;
            Name = name;
            Fps = null;
            IsSelected = false;
        }
        public CameraInfo(CameraScan c)
        {
            Number = c.number;
            Name = c.name;
            Fps = null;
            IsSelected = false;
        }
    }
}
