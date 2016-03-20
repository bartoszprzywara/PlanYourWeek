using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Remonty
{
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            SetControls();
        }

        private void SetControls()
        {
            StartDayTimePicker.Time = new TimeSpan(07, 00, 00);
            StartWorkingTimePicker.Time = new TimeSpan(09, 00, 00);
            EndWorkingTimePicker.Time = new TimeSpan(17, 00, 00);
            EndDayTimePicker.Time = new TimeSpan(23,00,00);
        }
    }
}
