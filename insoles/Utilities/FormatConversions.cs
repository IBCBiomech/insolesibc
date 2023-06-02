using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Emgu.CV;

namespace insoles.Utilities
{
    public static class FormatConversions
    {
        public static BitmapSource ToBitmapSource(Mat mat)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                mat.ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
