using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Remonty
{
    public sealed partial class SimpleListOfContexts : UserControl
    {
        public Models.Context Context { get { return this.DataContext as Models.Context; } }

        public SimpleListOfContexts()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteButton.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Collapsed;

            TitleTextBlock.Visibility = Visibility.Collapsed;
            TitleTextBox.Visibility = Visibility.Visible;

            //var ItemID = ((FrameworkElement)e.OriginalSource).DataContext;
            //var dialog = new MessageDialog(ItemID.ToString());
            //await dialog.ShowAsync();

            // http://www.familie-smits.com/development-tips/tinder-control-for-your-windows-app
        }
    }
}
