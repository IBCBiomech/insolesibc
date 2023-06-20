using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Utilities
{
    public static class HelperFunctions
    {
        public static Tuple<double, double> Average(List<Tuple<int, int>> values)
        {
            double x = 0;
            double y = 0;
            foreach (Tuple<int, int> item in values)
            {
                x += item.Item1;
                y += item.Item2;
            }
            x /= values.Count;
            y /= values.Count;
            return new Tuple<double, double>(x, y);
        }
        public static Color Interpolate(Color color1, Color color2, float ratio = 0.5f)
        {
            float ratio2 = 1 - ratio;
            return Color.FromArgb(
                (int)(color1.R * ratio + color2.R * ratio2),
                (int)(color1.G * ratio + color2.G * ratio2),
                (int)(color1.B * ratio + color2.B * ratio2)
            );
        }
        public static Color Interpolate(Color color1, Color color2, int alpha)
        {
            const float ratio = 0.5f;
            return Color.FromArgb(
                alpha,
                (int)(color1.R * ratio + color2.R * ratio),
                (int)(color1.G * ratio + color2.G * ratio),
                (int)(color1.B * ratio + color2.B * ratio)
            );
        }
    }
}
