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

namespace Remonty
{
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddEditActivity));
            TitleTextBlock.Text = "Nowe zadanie";
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

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (YourWeekListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Twój tydzień";
                ContentFrame.Navigate(typeof(YourWeek));
                MenuSplitView.IsPaneOpen = false;
            }
            else if (IncomingListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Nowe";
                ContentFrame.Navigate(typeof(Incoming));
                MenuSplitView.IsPaneOpen = false;
            }
            else if (ScheduledListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Zaplanowane";
                ContentFrame.Navigate(typeof(Settings), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
            }
            else if (NextListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Najbliższe";
                ContentFrame.Navigate(typeof(Settings), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
            }
            else if (SomedayListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Kiedyś";
                ContentFrame.Navigate(typeof(Settings), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
            }
            else if (WaitingListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Oddelegowane";
                ContentFrame.Navigate(typeof(Settings), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
            }
            else if (DoneListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Zrobione";
                ContentFrame.Navigate(typeof(Settings), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
            }
            else if (ProjectsListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Projekty";
                ContentFrame.Navigate(typeof(Settings), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
            }
            else if (ContextsListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Konteksty";
                ContentFrame.Navigate(typeof(Settings), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
            }
            else if (SettingsListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Ustawienia";
                ContentFrame.Navigate(typeof(Settings), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
            }
        }
    }
}
