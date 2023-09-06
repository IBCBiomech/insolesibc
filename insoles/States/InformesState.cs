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
    public class InformesState : INotifyPropertyChanged
    {
        private string path;
        
        public string Path { get { return path; } set { path = value; OnPropertyChanged(); } }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
      
    }
}
