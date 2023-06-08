using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Graphs
{
    public abstract class Plantilla
    {
        protected Dictionary<Sensor, Size> sensors = new Dictionary<Sensor, Size>();
        public Size size{get; protected set;}
        protected double modificator;
        protected double drawingHeight;
        public Plantilla(Size size, double drawingHeight) 
        { 
            this.size = size;
            this.drawingHeight = drawingHeight;
            this.modificator = drawingHeight / size.Height;
        }
        public abstract Size GetSize(Sensor sensor);
        public abstract double GetArea(Sensor sensor);
    }
    public class PlantillaWiseware: Plantilla
    {
        public PlantillaWiseware(float drawingHeight) : base(new Size(9, 26), drawingHeight) 
        {
            double width = 2;
            double height = 3.5;
            sensors.Add(Sensor.HEEL_R, new Size(width, height));
            sensors.Add(Sensor.HEEL_L, new Size(width, height));
            sensors.Add(Sensor.ARCH, new Size(width, height));
            sensors.Add(Sensor.MET1, new Size(width, height));
            sensors.Add(Sensor.MET3, new Size(width, height));
            sensors.Add(Sensor.MET5, new Size(width, height));
            sensors.Add(Sensor.TOES, new Size(width, height));
            sensors.Add(Sensor.HALLUX, new Size(width, height));
        }

        public override double GetArea(Sensor sensor)
        {
            return sensors[sensor].Width * modificator * sensors[sensor].Height * modificator;
        }

        public override Size GetSize(Sensor sensor)
        {
            return new Size(sensors[sensor].Width * modificator, sensors[sensor].Height * modificator);
        }
    }
}
