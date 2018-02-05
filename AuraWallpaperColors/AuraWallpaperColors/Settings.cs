using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraWallpaperColors
{
    public struct Settings
    {
        public int TransitionLength;
        public int NumPaletteColors;
        public int MainColorThreshold;
        public int ContrastConstant;
    }
    
    public static class SettingsUtils
    {
        
        public static Settings ReadSettings() {
            return new Settings
            {
                TransitionLength = Properties.Settings.Default.TransitionLength,
                NumPaletteColors = Properties.Settings.Default.NumPaletteColors,
                ContrastConstant = Properties.Settings.Default.ContrastConstant,
                MainColorThreshold = Properties.Settings.Default.MainColorThreshold
            };
        }

        public static void SaveSettings(Settings s) {
            Properties.Settings.Default.TransitionLength = s.TransitionLength;
            Properties.Settings.Default.NumPaletteColors = s.NumPaletteColors;
            Properties.Settings.Default.ContrastConstant = s.ContrastConstant;
            Properties.Settings.Default.MainColorThreshold = s.MainColorThreshold;
            Properties.Settings.Default.Save();
        }

    }
}
