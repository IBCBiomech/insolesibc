using insoles.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace insoles.States
{
    public class AnalisisState : INotifyPropertyChanged
    {
        private Test? _test;
        public Test? test { get { return _test; }  set { _test = value; OnPropertyChanged(); } }
        public float peso;
        public bool paused = true;
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public int framesTaken { get; set; }
    }
}
