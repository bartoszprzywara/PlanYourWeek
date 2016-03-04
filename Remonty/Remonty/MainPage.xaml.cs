using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Remonty
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(YourWeek));
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MenuSplitView.IsPaneOpen = !MenuSplitView.IsPaneOpen;
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (YourWeekListBoxItem.IsSelected)
            {
                ContentFrame.Navigate(typeof(YourWeek));
                TitleTextBlock.Text = "Twój tydzień";
                MenuSplitView.IsPaneOpen = false;
            }
            else if (IncomingListBoxItem.IsSelected)
            {
                ContentFrame.Navigate(typeof(Incoming));
                TitleTextBlock.Text = "Nowe";
                MenuSplitView.IsPaneOpen = false;
            }
            else if (SettingsListBoxItem.IsSelected)
            {
                ContentFrame.Navigate(typeof(Settings));
                TitleTextBlock.Text = "Ustawienia";
                MenuSplitView.IsPaneOpen = false;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchSplitView.IsPaneOpen = !SearchSplitView.IsPaneOpen;
            SearchSplitTextBox.Text = String.Empty;
        }

        private void SearchSplitButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(SearchResults), SearchSplitTextBox.Text);
            TitleTextBlock.Text = "Wyniki";
            SearchSplitView.IsPaneOpen = false;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(AddActivity));
            TitleTextBlock.Text = "Nowe zadanie";
        }
    }
}
