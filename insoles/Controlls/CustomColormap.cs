using ScottPlot.Drawing;
using System;
using System.Drawing;

namespace insoles.Controlls
{
    class CustomColormap : IColormap
    {
        private Func<double, Color> colorFunc;
        private string name;

        public CustomColormap(Func<double, Color> colorFunc, string name)
        {
            this.colorFunc = colorFunc;
            this.name = name;
        }

        public string Name => name;

        public Color GetColor(double value)
        {
            return colorFunc(value);
        }

        public (byte r, byte g, byte b) GetRGB(byte value)
        {
            Color color = GetColor(value / 255.0);
            return (color.R, color.G, color.B);
        }
    }
}
