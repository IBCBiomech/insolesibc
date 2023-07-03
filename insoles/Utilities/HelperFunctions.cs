using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows;
using Color = System.Drawing.Color;

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
        public static double?[,] replace(double[,] array, double value, double? replacement)
        {
            double?[,] result = new double?[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i, j] == value)
                    {
                        result[i, j] = replacement;
                    }
                    else
                    {
                        result[i, j] = array[i, j];
                    }
                }
            }
            return result;
        }
        public static int SquareDistance(int x1, int y1, int x2, int y2)
        {
            int dist_x = x1 - x2;
            int dist_y = y1 - y2;
            dist_x = dist_x * dist_x;
            dist_y = dist_y * dist_y;
            return dist_x + dist_y;
        }
        public static float SquareDistance(float x1, float y1, float x2, float y2)
        {
            float dist_x = x1 - x2;
            float dist_y = y1 - y2;
            dist_x = dist_x * dist_x;
            dist_y = dist_y * dist_y;
            return dist_x + dist_y;
        }
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                T foundChild = FindVisualChild<T>(child);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }

            return null;
        }

    }
}
