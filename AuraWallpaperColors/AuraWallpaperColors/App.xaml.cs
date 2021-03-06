﻿using fi.ossiv.AuraSDK;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AuraWallpaperColors
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        int NumPaletteColors = 10;
        int MainColorThreshold = 128;
        int ContrastConstant = 128;

        ColorCarousel ColorCarousel = null;

        WallpaperWatcher WallpaperWatcher;

        IDisposable WallpaperSubscription;

        AuraColorSink AuraSink;

        TaskbarIcon TrayIcon;

        MainWindow SettingsWindow;

        public Settings Settings { get; private set; }


        [STAThread]
        public static void Main()
        {
            string mutexName = System.Reflection.Assembly.GetExecutingAssembly().GetType().GUID.ToString();
            if (SingleInstance<App>.InitializeAsFirstInstance(mutexName))
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();
                SingleInstance<App>.Cleanup();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            InitializeSettings();
            InitializeTrayIcon();
            var ctx = SynchronizationContext.Current;
            Task.Run ( () => InitializeAura(ctx));
            

        }

        private void InitializeSettings()
        {
            SetSettings(SettingsUtils.ReadSettings());
        }

        public void SetSettings(Settings settings)
        {
            this.Settings = settings;

            if (ColorCarousel != null)
            {
                ColorCarousel.TransitionDuration = Settings.TransitionLength;
            }
            NumPaletteColors = Settings.NumPaletteColors;
            MainColorThreshold = Settings.MainColorThreshold;
            ContrastConstant = Settings.ContrastConstant;


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
            else
            {
                SettingsWindow.Activate();
            }
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            SettingsWindow.Closed -= SettingsWindow_Closed;
            SettingsWindow = null;
        }

        async Task InitializeAura(SynchronizationContext context)
        {

            while (Aura.GetMotherboards().Count < 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            AuraSink = new AuraColorSink(context);
            WallpaperWatcher = new WallpaperWatcher();



            WallpaperSubscription = WallpaperWatcher.Subscribe(WallpaperPathChanged);
        }

        bool SettingColor;
        private void WallpaperPathChanged(string newPath)
        {
            if (SettingColor) return;
            SettingColor = true;
            Observable.FromAsync(() => SetColorsFromImage(newPath)).Take(1).Subscribe(_ =>
            {
                SettingColor = false;
            });
        }

        private async Task SetColorsFromImage(string path)
        {
            if (path == null || !File.Exists(path))
            {
                return;
            }

            var colors = await ColorQuantizer.GetPaletteFromImageFile(path, NumPaletteColors, MainColorThreshold, ContrastConstant);

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

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            return true;
        }
    }
}
