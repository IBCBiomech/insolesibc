using mvvm.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Models
{
    public class CameraInfo
    {
        public string name { get;set; }
        public int number { get; set; }
        public int? fps { get; set; }
        public CameraInfo(int number, string name)
        {
            this.number = number;
            this.name = name;
            this.fps = null;
        }
        public CameraInfo(CameraScanMessage c)
        {
            this.number = c.number;
            this.name = c.name;
            this.fps = null;
        }
    }
}
