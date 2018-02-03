using fi.ossiv.AuraSDK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuraWallpaperColors
{
    public interface IColorSink
    {
        int ColorCount { get; }
        Color this[int index] { set; }
        void Refresh();
    }


    public class ColorCarousel
    {

        Color OldOuterColor;
        Color OuterColor;

        List<Color> Colors;
        int NextColorIndex = 0;

        object colorLock = new object();
        List<Color> OldColors;


        public void SetColors(Color outer, List<Color> colors)
        {
            if (colors.Count < Sink.ColorCount - 1)
            {
                throw new ArgumentException("Too few colors for sink", "colors");
            }

            lock (colorLock)
            {
                OuterColor = outer;
                Colors = colors;
                NextColorIndex = 0;
            }
        }

        public double TransitionDuration { get; set; } = 1000;
        IColorSink Sink;
       
        public ColorCarousel(IColorSink sink)
        {
            Sink = sink;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await AnimateNext();
                    Thread.Sleep(TimeSpan.Zero);
                }
            });
        }

        void SetColorsInternal(Color outer, List<Color> colors)
        {
            var numColors = Sink.ColorCount;


            foreach (var i in Enumerable.Range(0, numColors - 1))
            {
                Sink[i] = colors[i];
            }

            Sink[numColors - 1] = outer;


            OldOuterColor = outer;
            OldColors = colors;
            Sink.Refresh();
        }

        async Task AnimateNext()
        {
            List<Color> nextColors = null;
            List<Color> currentColors = null;
            Color currentOuter;
            Color nextOuter;
            lock (colorLock)
            {
                currentOuter = OldOuterColor;
                nextOuter = OuterColor;
                currentColors = OldColors;
                nextColors = Enumerable.Range(NextColorIndex, Sink.ColorCount - 1).Select(i => Colors[i % Colors.Count]).ToList();
            }

            if (currentColors == null)
            {
                SetColorsInternal(OuterColor, nextColors);
            }
            else
            {
                await AnimateColorTransition(TransitionDuration, currentOuter, currentColors, nextOuter, nextColors);
            }

            lock (colorLock)
            {
                NextColorIndex = (NextColorIndex + 1) % Colors.Count;
            }

        }


        async Task<bool> AnimateColorTransition(double durationMillis, Color outerFrom, List<Color> from, Color outerTo, List<Color> to)
        {



            if (durationMillis < 16.7)
            {
                return false;
            }

            while (from.Count > to.Count)
            {
                to.Add(to[0]);
            }

            var tcs = new TaskCompletionSource<bool>();


            double elapsedMillis = 0;
            Timer timer = null;
            bool first = true;
            timer = new Timer((_) =>
            {

                if (!first)
                {
                    elapsedMillis += 16.7;
                }
                else
                {
                    first = false;
                }

                double fraction = elapsedMillis / durationMillis;

                SetColorsInternal(outerFrom.Mix(outerTo, fraction), from.Mix(to, fraction));

                if (elapsedMillis >= durationMillis)
                {
                    tcs.TrySetResult(true);
                    timer?.Dispose();
                }

            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(16.7));


            return await tcs.Task;
        }



    }
}
