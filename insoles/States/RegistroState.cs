using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.States
{
    public class RegistroState
    {
        public bool paused { get; set; } = false;
        public bool capturing { get; set; } = false;
        public bool recording { get; set; } = false;
    }
}
