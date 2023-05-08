using insolesMVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insolesMVVM.Messages
{
    public class OpenCameraSelectedMessage : Message
    {
        public Camera camera { get; set; }
        public OpenCameraSelectedMessage(Camera camera) 
        {
            this.camera = camera;
        }
    }
}
