using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Messages
{
    public class CameraScan
    {
        public string name { get; set; }
        public int number { get; set; }
        public List<int> fps { get; set; }
        public Dictionary<int, List<System.Drawing.Size>> resolutions { get; set; }
        public CameraScan(int number, string name, List<int> fps, Dictionary<int, List<System.Drawing.Size>> resolutions)
        {
            this.number = number;
            this.name = name;
            this.fps = fps;
            this.resolutions = resolutions;
        }
    }
}
