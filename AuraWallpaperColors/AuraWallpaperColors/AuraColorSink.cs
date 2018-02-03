using fi.ossiv.AuraSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace AuraWallpaperColors
{
    public class AuraColorSink : IColorSink
    {
        Aura.Motherboard Motherboard;
        SynchronizationContext Context;
        public AuraColorSink()
        {
            Context = SynchronizationContext.Current;
            Motherboard = Aura.GetMotherboards().FirstOrDefault();
            if (Motherboard == null)
            {
                throw new InvalidOperationException("No Aura motherboard found");
            }
            Motherboard.SetMode(Aura.Motherboard.Mode.SoftwareControl);
            Motherboard.ReadLeds();
            Colors = new Color[Motherboard.Leds.Count];
        }

        Color[] Colors;

        Color IColorSink.this[int index]
        {
            set
            {
                Colors[index] = value;
            }
        }

        int IColorSink.ColorCount => Colors.Length;


        // try to work around led color reproduction issues by increasing contrast
        byte ApplyContrast(byte color)
        {
            const int contrastFactor = 128;
            const double factor = (259.0 * (255 + contrastFactor)) / (255 * (259 - contrastFactor));

            var resultVal = factor * (color - 128) + 128;

            return (byte)(Math.Max(Math.Min(resultVal, 255), 0));
        }

        void IColorSink.Refresh()
        {
            Context.Post((_) =>
            {
                foreach (var (led, index) in Motherboard.Leds.Enumerate())
                {
                    var color = Colors[index];
                    led.Red = ApplyContrast(color.R);
                    led.Green = ApplyContrast(color.G);
                    led.Blue = ApplyContrast(color.B);
                }
                Motherboard.WriteLeds();
            }, null);
        }
    }
}
