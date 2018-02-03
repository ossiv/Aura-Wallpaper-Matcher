using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;

using Color = System.Drawing.Color;
using System.Security.Principal;
using Leptonica;

namespace AuraWallpaperColors
{



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        int numPaletteColors = 10;

        ColorCarousel ColorCarousel = null;

        WallpaperWatcher WallpaperWatcher;


        IDisposable WallpaperSubscription;

        AuraColorSink AuraSink;

        public MainWindow()
        {
            InitializeComponent();

            AuraSink = new AuraColorSink();
            WallpaperWatcher = new WallpaperWatcher();

            WallpaperSubscription = WallpaperWatcher.Subscribe(WallpaperPathChanged);
        }

        private void WallpaperPathChanged(string newPath)
        {
            if (newPath == null || !File.Exists(newPath))
            {
                return;
            }

            var colors = ColorQuantizer.GetPaletteFromImageFile(newPath, numPaletteColors);

            if (ColorCarousel == null)
            {
                ColorCarousel = new ColorCarousel(AuraSink);
                ColorCarousel.SetColors(colors.outer, colors.colors);
                ColorCarousel.Start();
            }
            else
            {
                ColorCarousel.SetColors(colors.outer, colors.colors);
            }

        }

    }
}
