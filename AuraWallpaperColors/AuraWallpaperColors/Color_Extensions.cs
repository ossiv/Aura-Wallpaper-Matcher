using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraWallpaperColors
{
    using SDColor = System.Drawing.Color;
    using SWMColor = System.Windows.Media.Color;


    public static class ColorExt
    {
        public static SWMColor ToSWMColor(this SDColor color) => SWMColor.FromArgb(color.A, color.R, color.G, color.B);
        public static SDColor ToSDColor(this SWMColor color) => SDColor.FromArgb(color.A, color.R, color.G, color.B);

        public static SDColor Mix(this SDColor first, SDColor second, double fraction)
        {
            if (first == null)
                throw new NullReferenceException();
            fraction = Math.Max(Math.Min(fraction, 1), 0);

            return SDColor.FromArgb(
                (byte)(first.R * (1 - fraction) + fraction * second.R),
                (byte)(first.G * (1 - fraction) + fraction * second.G),
                (byte)(first.B * (1 - fraction) + fraction * second.B));
        }

        public static List<SDColor> Mix(this List<SDColor> first, List<SDColor> second, double fraction) {
            if (first == null)
                throw new NullReferenceException();

            if (first.Count < second.Count) {
                throw new InvalidOperationException("second must be at least as long as first");
            }

            return first.Select((c, i) => c.Mix(second[i], fraction)).ToList();
        }

    }



}
