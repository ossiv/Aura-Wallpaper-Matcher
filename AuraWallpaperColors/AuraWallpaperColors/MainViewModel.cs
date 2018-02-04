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
        public int TransitionLength {
            get => transitionLength;
            set { transitionLength = value; NotifyPropertyChanged(); }
        }

        int numPaletteColors = 10;
        public int NumPaletteColors
        {
            get => numPaletteColors;
            set { numPaletteColors = value; NotifyPropertyChanged(); }
        }


        public MainViewModel() {
            PropertyChanged += MainViewModel_PropertyChanged;
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(TransitionLength):
                    IsDirty = true;
                    break;
                case nameof(NumPaletteColors):
                    IsDirty = true;
                    break;
            }
        }

        internal void ApplyClicked()
        {
            if (!IsDirty) {
                return;
            }
        }
    }
}
