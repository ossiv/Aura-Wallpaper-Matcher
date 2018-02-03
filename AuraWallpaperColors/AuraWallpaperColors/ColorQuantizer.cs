using Leptonica;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuraWallpaperColors
{
    public static class ColorQuantizer
    {
        public static bool IsFileReady(String sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return (inputStream.Length > 0);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static (Color outer, List<Color> colors) GetPaletteFromImageFile(string path, int numColors)
        {
            if (numColors < 2) {
                numColors = 2;
            }

            // we're trying to get at the file pretty fast after it changes
            // so sometimes it's still locked by another process
            while (!IsFileReady(path))
            {
                Thread.Sleep(TimeSpan.Zero);
            }

            Pix pix = null;

            // if reading fails, the pix will be null
            // but this is probably because someone else is touching the file
            // so just try again after a while
            while (pix == null) 
            {
                pix = Pix.Read(path);
                if (pix == null)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
            }

           var colors = ExtractColors(pix);
            // the first one should be the "base" color for the image,
            // so use that as the main color
            return (colors[0], colors.Skip(1).ToList());
        }

        static List<Color> ExtractColors(Pix from)
        {

            var outpix = Leptonica.ColorQuant2.pixMedianCutQuantGeneral(from, 0, 8, 10, 0, 0, 0);
            var colormap = outpix.Colormap;
            var numColors = Leptonica.Colormap.pixcmapGetCount(colormap);

            List<Color> colors = new List<Color>();

            for (int i = 0; i < numColors; ++i)
            {
                int r = 0, g = 0, b = 0;
                Colormap.pixcmapGetColor(colormap, i, out r, out g, out b);

                int color = b | g << 8 | r << 16 | (0xFF << 24);
                colors.Add(Color.FromArgb(color));
            }


            return colors;
        }
    }
}
