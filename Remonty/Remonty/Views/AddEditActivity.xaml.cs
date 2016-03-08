using Remonty.Models;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Remonty
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddEditActivity : Page
    {
        public AddEditActivity()
        {
            this.InitializeComponent();

            PriorComboBox.ItemsSource = new string[] {"Niski","Normalny","Wysoki"};
            EstimateComboBox.ItemsSource = new string[] {"10 min","20 min","30 min","1 godz","2 godz","3 godz","4 godz","6 godz","10 godz"};
            ContextComboBox.ItemsSource = ContextManager.getContexts();
            ProjectComboBox.ItemsSource = ProjectManager.getProjects();

            IsAllDayToggleSwitch.IsOn = true;
            StartHourRelativePanel.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;

            Activity activity = e.Parameter as Activity;

            TitleTextBlock.Text = (activity.Title != null) ? activity.Title : "Twoje zadanie";

            DoneButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            SaveButton.Click -= new RoutedEventHandler(SaveButton_Click);
            SaveButton.Click += new RoutedEventHandler(SaveExistingButton_Click);

            SetControlValues(activity);
        }

        private void SetControlValues(Activity activity)
        {
            if (activity.Title != null)
                TitleTextBox.Text = activity.Title;
            if (activity.Description != null)
                DescriptionTextBox.Text = activity.Description;
            PriorComboBox.SelectedItem = activity.Priority;
            if (activity.IsAllDay != null)
                IsAllDayToggleSwitch.IsOn = (bool)activity.IsAllDay;
            if (activity.StartHour != null)
                StartHourTimePicker.Time = (TimeSpan)activity.StartHour;
            StartDatePicker.Date = activity.StartDate;
            EndDatePicker.Date = activity.EndDate;
            EstimateComboBox.SelectedItem = activity.Estimation;
            ContextComboBox.SelectedItem = activity.Context;
            ProjectComboBox.SelectedItem = activity.Project;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        async private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Dodaję nowy");
            await dialog.ShowAsync();
        }

        async private void SaveExistingButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Edytuję istniejący");
            await dialog.ShowAsync();
        }

        private void IsAllDayToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (IsAllDayToggleSwitch.IsOn)
                StartHourRelativePanel.Visibility = Visibility.Collapsed;
            else
                StartHourRelativePanel.Visibility = Visibility.Visible;
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            string _title = TitleTextBox.Text;
            string _description = DescriptionTextBox.Text;
            string _priority = (PriorComboBox != null) ? (string)(PriorComboBox.SelectedItem) : "";
            bool _isAllDay = IsAllDayToggleSwitch.IsOn;
            string _startHour = StartHourTimePicker.Time.ToString();
            string _startDate = (StartDatePicker.Date != null) ? ((DateTimeOffset)StartDatePicker.Date).ToString("dd.MM.yyyy") : "";
            string _endDate = (EndDatePicker.Date != null) ? ((DateTimeOffset)EndDatePicker.Date).ToString("dd.MM.yyyy") : "";
            string _estimation = (EstimateComboBox != null) ? (string)(EstimateComboBox.SelectedItem) : "";
            string _context = (ContextComboBox.SelectedItem != null) ? (string)(ContextComboBox.SelectedItem) : "";
            string _project = (ProjectComboBox != null) ? (string)(ProjectComboBox.SelectedItem) : "";

            Podsumowanie.Text = "Tytuł: " + _title + "\t" +
                                "Opis: " + _description + "\n" +
                                "Prior: " + _priority + "\t" +
                                "CzyCałyDzień: " + _isAllDay + "\t" +
                                "Godzina: " + _startHour + "\n" +
                                "Start: " + _startDate + "\n" +
                                "Kuniec: " + _endDate + "\n" +
                                "Estim: " + _estimation + "\t" +
                                "Kontekst: " + _context + "\t" +
                                "Projekt: " + _project;
        }
    }
}
