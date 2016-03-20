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

            if (App.CurrentFrameInMainPage != null)
            {
                TitleTextBlock.Text = App.TitleTextBlockText;
                ContentFrame.Navigate(App.CurrentFrameInMainPage, TitleTextBlock.Text);
                // TODO: Pamiętać, że aktualnie nie działa dla powrotu do SearchResults
            }
            else
                ContentFrame.Navigate(typeof(YourWeek));
        }

        private void SaveState()
        {
            App.CurrentFrameInMainPage = ContentFrame.CurrentSourcePageType;
            App.TitleTextBlockText = TitleTextBlock.Text;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MenuSplitView.IsPaneOpen = !MenuSplitView.IsPaneOpen;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            SaveState();
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
                SaveState();
            }
            else if (IncomingListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Nowe";
                ContentFrame.Navigate(typeof(ActivityGeneric), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
                SaveState();
            }
            else if (ScheduledListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Zaplanowane";
                ContentFrame.Navigate(typeof(ActivityGeneric), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
                SaveState();
            }
            else if (NextListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Najblizsze"; // nie działają mi polskie znaki w sqlite
                ContentFrame.Navigate(typeof(ActivityGeneric), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
                SaveState();
            }
            else if (SomedayListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Kiedys"; // nie działają mi polskie znaki w sqlite
                ContentFrame.Navigate(typeof(ActivityGeneric), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
                SaveState();
            }
            else if (WaitingListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Oddelegowane";
                ContentFrame.Navigate(typeof(ActivityGeneric), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
                SaveState();
            }
            else if (DoneListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Zrobione";
                ContentFrame.Navigate(typeof(ActivityGeneric), TitleTextBlock.Text);
                MenuSplitView.IsPaneOpen = false;
                SaveState();
            }
            else if (ProjectsListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Projekty";
                ContentFrame.Navigate(typeof(Projects));
                MenuSplitView.IsPaneOpen = false;
                SaveState();
            }
            else if (ContextsListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Konteksty";
                ContentFrame.Navigate(typeof(Contexts));
                MenuSplitView.IsPaneOpen = false;
                SaveState();
            }
            else if (SettingsListBoxItem.IsSelected)
            {
                TitleTextBlock.Text = "Ustawienia";
                ContentFrame.Navigate(typeof(Settings));
                MenuSplitView.IsPaneOpen = false;
                SaveState();
            }
        }
    }
}
