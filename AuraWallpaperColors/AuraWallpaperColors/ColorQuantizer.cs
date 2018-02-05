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


        public static async Task<(Color outer, List<Color> colors)> GetPaletteFromImageFile(string path, int numColors, int mainColorThreshold, int contrastConstant)
        {
            if (numColors < 2)
            {
                numColors = 2;
            }

            // we're trying to get at the file pretty fast after it changes
            // so usually it's still locked by another process
            while (!IsFileReady(path))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }

            Pix pix = null;
            List<Color> colors = null;
            try
            {
                // if reading fails, the pix will be null
                // but this is probably because someone else is touching the file
                // so just try again after a while
                while (pix == null)
                {
                    pix = Pix.Read(path);
                    if (pix == null)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(200));
                    }
                }

                colors = ExtractColors(pix, numColors, contrastConstant);
            }
            finally
            {
                pix?.Dispose();
            }

            Color outer = colors[0];
            for (int i = 0; i < colors.Count; ++i) {
                var c = colors[i];
                if (c.R + c.B + c.G > mainColorThreshold) {
                    outer = c;
                    colors.RemoveAt(i);
                    break;
                }
            }
            if (colors.Count == 0) {
                colors.Add(outer);
            }
            return (outer, colors);
        }
        // try to work around led color reproduction issues by increasing contrast
        static byte ApplyContrast(byte color, int contrastConstant)
        {
            var contrastFactor = contrastConstant;
            double factor = (259.0 * (255 + contrastFactor)) / (255 * (259 - contrastFactor));

            var resultVal = factor * (color - 128) + 128;

            return (byte)(Math.Max(Math.Min(resultVal, 255), 0));
        }

        static List<Color> ExtractColors(Pix from, int numPaletteColors, int contrastConstant)
        {
            List<Color> colors = new List<Color>();

            using (var outpix = Leptonica.ColorQuant2.pixMedianCutQuantGeneral(from, 0, 8, numPaletteColors, 0, 0, 0))
            {
                var colormap = outpix.Colormap;
                var numColors = Leptonica.Colormap.pixcmapGetCount(colormap);


                for (int i = 0; i < numColors; ++i)
                {
                    Colormap.pixcmapGetColor(colormap, i, out int r, out int g, out int b);

                    int color = ApplyContrast((byte)b, contrastConstant) 
                        | ApplyContrast((byte)g , contrastConstant) << 8
                        | ApplyContrast((byte)r, contrastConstant) << 16 
                        | (0xFF << 24);
                    colors.Add(Color.FromArgb(color));
                }
            }

            return colors;
        }
    }
}
