using MathNet.Numerics.LinearAlgebra;
using System.Drawing;

namespace insoles.Utilities
{
    public static class MathNetHelpers
    {
        public static byte ColorToByte(Color color)
        {
            return (byte)((color.R + color.G + color.B) / 3);
        }
        public static Matrix<float> ImageToMatrix(Bitmap image)
        {
            Matrix<float> floats = Matrix<float>.Build.Dense(image.Width, image.Height);
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    floats[i, j] = ColorToByte(image.GetPixel(i, j));
                }
            }
            return floats;
        }
    }
}
