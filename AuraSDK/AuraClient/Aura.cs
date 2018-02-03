using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace fi.ossiv.AuraSDK
{
    public class Aura
    {
        [DllImport("AURA_SDK.dll")]
        private static extern UInt32 EnumerateMbController(IntPtr[] handles, UInt32 size);

        [DllImport("AURA_SDK.dll")]
        private static extern UInt32 SetMbMode(IntPtr handle, UInt32 mode);

        [DllImport("AURA_SDK.dll")]
        private static extern UInt32 GetMbLedCount(IntPtr mb);

        [DllImport("AURA_SDK.dll")]
        private static extern UInt32 GetMbColor(IntPtr mb, byte[] colorsOut, UInt32 size);

        [DllImport("AURA_SDK.dll")]
        private static extern UInt32 SetMbColor(IntPtr mb, byte[] colorsIn, UInt32 size);

        const int BytesPerColor = 3;

        public class Led
        {
            const int RedIndex = 0;
            const int GreenIndex = 2;
            const int BlueIndex = 1;

            public byte[] Color { get; }
            public byte Red
            {
                get { return Color[RedIndex]; }
                set { Color[RedIndex] = value; }
            }
            public byte Green
            {
                get { return Color[GreenIndex]; }
                set { Color[GreenIndex] = value; }
            }
            public byte Blue
            {
                get { return Color[BlueIndex]; }
                set { Color[BlueIndex] = value; }
            }
            private int Index;

            private Led(int index, byte[] color)
            {
                Index = index;
                Color = color;
            }

            public static Led FromIndexAndColors(int index, byte[] colors)
            {
                var color = new byte[BytesPerColor];

                CopyColorFromIndex(index, color, colors);

                return new Led(index, color);
            }
        }

        static void CopyColorFromIndex(int index, byte[] colorOut, byte[] colorsIn)
        {
            for (int j = 0; j < BytesPerColor; ++j)
            {
                colorOut[j] = colorsIn[index * BytesPerColor + j];
            }
        }

        public class Motherboard
        {
            public enum Mode
            {
                DefaultProgram = 0, SoftwareControl = 1
            }

            IntPtr Handle { get; }

            List<Led> LedsInternal { get; set; }

            public IReadOnlyList<Led> Leds { get { return LedsInternal; } }

            public Motherboard(IntPtr handle)
            {
                Handle = handle;
            }

            byte[] colorsBuffer;

            public void SetMode(Mode m)
            {
                SetMbMode(Handle, (UInt32)m);
            }

            public void ReadLeds()
            {
                var count = GetMbLedCount(Handle);

                if (colorsBuffer == null)
                {
                    colorsBuffer = new byte[BytesPerColor * count];
                }

                GetMbColor(Handle, colorsBuffer, (UInt32)colorsBuffer.Length);

                if (LedsInternal == null)
                {
                    LedsInternal = new List<Led>();

                    for (int i = 0; i < count; ++i)
                    {
                        LedsInternal.Add(Led.FromIndexAndColors(i, colorsBuffer));
                    }
                }
                else
                {
                    foreach (var (led, index) in LedsInternal.Select((led, i) => (led, i)))
                    {
                        CopyColorFromIndex(index, led.Color, colorsBuffer);
                    }
                }
            }

            public void WriteLeds()
            {
                for (int i = 0; i < LedsInternal.Count; ++i)
                {
                    for (int j = 0; j < BytesPerColor; ++j)
                    {
                        colorsBuffer[i * BytesPerColor + j] = LedsInternal[i].Color[j];
                    }
                }

                SetMbColor(Handle, colorsBuffer, (UInt32)colorsBuffer.Length);
            }
        }

        public static IList<Motherboard> GetMotherboards()
        {
            var controllerCount = EnumerateMbController(null, 0);
            IntPtr[] controllers = new IntPtr[controllerCount];
            EnumerateMbController(controllers, controllerCount);
            return controllers.Select(i => new Motherboard(i)).ToList();
        }
    }
}
