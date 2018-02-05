using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AuraWallpaperColors
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public bool IsInDesignMode
        {
            get
            {
                return DesignerProperties.GetIsInDesignMode(new DependencyObject());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainViewModel : ViewModelBase
    {


        bool isDirty = false;
        public bool IsDirty
        {
            get => isDirty;
            set { isDirty = value; NotifyPropertyChanged(); }
        }

        int transitionLength = 1000;
        public int TransitionLength
        {
            get => transitionLength;
            set { transitionLength = value; NotifyPropertyChanged(); }
        }

        int numPaletteColors = 10;
        public int NumPaletteColors
        {
            get => numPaletteColors;
            set { numPaletteColors = value; NotifyPropertyChanged(); }
        }
        int mainColorThreshold = 128;
        public int MainColorThreshold
        {
            get => mainColorThreshold;
            set { mainColorThreshold = value; NotifyPropertyChanged(); }
        }
        int contrastConstant = 127;
        public int ContrastConstant
        {
            get => contrastConstant;
            set { contrastConstant = value; NotifyPropertyChanged(); }
        }

        public MainViewModel()
        {

            var app = (App.Current as App);
            var settings = app.Settings;

            transitionLength = settings.TransitionLength;
            numPaletteColors = settings.NumPaletteColors;
            contrastConstant = settings.ContrastConstant;
            mainColorThreshold = settings.MainColorThreshold;

            PropertyChanged += MainViewModel_PropertyChanged;
        }

        bool Validate()
        {
            return ContrastConstant >= 0 && ContrastConstant < 256;
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TransitionLength):
                case nameof(NumPaletteColors):
                case nameof(MainColorThreshold):
                case nameof(ContrastConstant):
                    IsDirty = Validate();
                    break;
            }
        }

        internal void ApplyClicked()
        {
            if (!IsDirty)
            {
                return;
            }
            var app = (App.Current as App);
            app.SetSettings(new Settings
            {
                TransitionLength = TransitionLength,
                NumPaletteColors = NumPaletteColors,
                MainColorThreshold = MainColorThreshold,
                ContrastConstant = ContrastConstant
            });
            IsDirty = false;
        }
    }
}
