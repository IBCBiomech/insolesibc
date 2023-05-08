using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insolesMVVM.Messages;
using ReactiveUI;

namespace insolesMVVM.Models
{
    public partial class Insole : ReactiveObject
    {
        private static HashSet<int> idsUsed = new();
        private static int getNextID()
        {
            for (int i = 0; i < idsUsed.Count; i++)
            {
                if (!idsUsed.Contains(i))
                {
                    idsUsed.Add(i);
                    return i;
                }
            }
            int id = idsUsed.Count;
            idsUsed.Add(id);
            return id;
        }
        private int id;
        public int Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }
        private string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
        private string address;
        public string Address
        {
            get => address;
            set => this.RaiseAndSetIfChanged(ref address, value);
        }
        public Insole(InsoleScan insole)
        {
            id = getNextID();
            name = insole.name;
            address = insole.address;
        }
    }
}
