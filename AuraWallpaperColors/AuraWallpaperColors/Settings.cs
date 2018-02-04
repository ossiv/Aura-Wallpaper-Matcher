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
    }
    
    public static class SettingsUtils
    {
        
        public static Settings ReadSettings() {
            return new Settings
            {
                TransitionLength = Properties.Settings.Default.TransitionLength,
                NumPaletteColors = Properties.Settings.Default.NumPaletteColors
            };
        }

        public static void SaveSettings(Settings s) {
            Properties.Settings.Default.TransitionLength = s.TransitionLength;
            Properties.Settings.Default.NumPaletteColors = s.NumPaletteColors;
            Properties.Settings.Default.Save();
        }

    }
}
