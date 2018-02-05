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
        

        public AuraColorSink(SynchronizationContext context)
        {
            Context = context;
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


        

        void IColorSink.Refresh()
        {
            Context.Post((_) =>
            {
                foreach (var (led, index) in Motherboard.Leds.Enumerate())
                {
                    var color = Colors[index];
                    led.Red = color.R;
                    led.Green = color.G;
                    led.Blue = color.B;
                }
                Motherboard.WriteLeds();
            }, null);
        }
    }
}
