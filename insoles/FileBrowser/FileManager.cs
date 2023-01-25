using System.Drawing;
using System.IO;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using insoles.FileBrowser.Enums;


// Devuelve los iconos de los ficheros
namespace insoles.FileBrowser
{
    public static class FileManager
    {
        // Devuelve el icono de un fichero
        public static ImageSource GetImageSource(string filename)
        {
            return GetImageSource(filename, new Size(16, 16));
        }
        // Devuelve el icono de un fichero de dimensiones size
        public static ImageSource GetImageSource(string filename, Size size)
        {
            using (var icon = ShellManager.GetIcon(Path.GetExtension(filename), ItemType.File, IconSize.Small, ItemState.Undefined))
            {
                return Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(size.Width, size.Height));
            }
        }
    }
}
