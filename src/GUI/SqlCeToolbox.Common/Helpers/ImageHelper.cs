using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public sealed class ImageHelper
    {
        private ImageHelper()
        {}

        public static Image GetImageFromResource(string relativeUriFileName)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(relativeUriFileName, UriKind.Relative);
            bitmap.EndInit();
            return new Image { Source = bitmap};
        }
    }
}