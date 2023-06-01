using insoles.Utilities;
using insoles.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class InsoleModel : ModelBase
    {
        public RegistroVM VM { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string MAC { get; set; }
        private bool _connected;
        public bool connected { 
            get 
            {
                return _connected;
            }
            set 
            {
                _connected = value;
                OnPropertyChanged();
            }
        }
        public InsoleModel(int id, string name, string MAC, RegistroVM VM)
        {
            this.id = id;
            this.name = name;
            this.MAC = MAC;
            this.VM = VM;
            this.connected = false;
        }
    }
}
