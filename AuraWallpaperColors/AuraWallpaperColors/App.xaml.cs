﻿using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AuraWallpaperColors
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        int numPaletteColors = 10;

        ColorCarousel ColorCarousel = null;

        WallpaperWatcher WallpaperWatcher;

        IDisposable WallpaperSubscription;

        AuraColorSink AuraSink;

        TaskbarIcon TrayIcon;

        MainWindow SettingsWindow;

        public Settings Settings { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            bool createdNew = false;
            string mutexName = System.Reflection.Assembly.GetExecutingAssembly().GetType().GUID.ToString();
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName, out createdNew))
            {
                if (!createdNew)
                {
                    // Only allow one instance
                    Application.Current.Shutdown();
                    return;
                }
            }
            InitializeSettings();
            InitializeAura();
            InitializeTrayIcon();

        }

        private void InitializeSettings()
        {
            SetSettings(SettingsUtils.ReadSettings());
        }

        public void SetSettings(Settings settings) {
            this.Settings = settings;

            if (ColorCarousel != null)
            {
                ColorCarousel.TransitionDuration = Settings.TransitionLength;
            }
            numPaletteColors = Settings.NumPaletteColors;

            SettingsUtils.SaveSettings(Settings);
        }

        private void InitializeTrayIcon()
        {
            TrayIcon = (TaskbarIcon)FindResource("TrayIcon");
            TrayIcon.TrayMouseDoubleClick += TrayIcon_TrayMouseDoubleClick;
        }

        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (SettingsWindow == null)
            {
                SettingsWindow = new MainWindow();

                SettingsWindow.Closed += SettingsWindow_Closed;

                SettingsWindow.Show();
            }
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            SettingsWindow.Closed -= SettingsWindow_Closed;
            SettingsWindow = null;
        }

        void InitializeAura()
        {
            AuraSink = new AuraColorSink();
            WallpaperWatcher = new WallpaperWatcher();

           

            WallpaperSubscription = WallpaperWatcher.Subscribe(WallpaperPathChanged);
        }

        bool SettingColor;
        private void WallpaperPathChanged(string newPath) {
            if (SettingColor) return;
            SettingColor = true;
            Observable.FromAsync(() => SetColorsFromImage(newPath)).Take(1).Subscribe(_ => {
                SettingColor = false;
            });
        }

        private async Task SetColorsFromImage(string path)
        {
            if (path == null || !File.Exists(path))
            {
                return;
            }

            var colors = await ColorQuantizer.GetPaletteFromImageFile(path, numPaletteColors);

            if (ColorCarousel == null)
            {
                ColorCarousel = new ColorCarousel(AuraSink)
                {
                    TransitionDuration = Settings.TransitionLength
                };
                ColorCarousel.SetColors(colors.outer, colors.colors);
                ColorCarousel.Start();
            }
            else
            {
                ColorCarousel.SetColors(colors.outer, colors.colors);
            }

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Current.Shutdown();
        }
    }
}
