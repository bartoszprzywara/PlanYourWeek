using Remonty.Models;
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
            IsAllDayToggleSwitch.IsOn = true;
            StartHourRelativePanel.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;
            Activity activity = e.Parameter as Activity;

            if (activity.Title != null)
                TitleTextBlock.Text = activity.Title;
            else
                TitleTextBlock.Text = "Twoje zadanie";
            DoneButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            
            if (activity.Title != null)
                TitleTextBox.Text = activity.Title;
            if (activity.Description != null)
                DescriptionTextBox.Text = activity.Description;
            if (activity.Priority != null)
                PriorComboBox.SelectedIndex = (int)activity.Priority;
            if (activity.IsAllDay != null)
                IsAllDayToggleSwitch.IsOn = (bool)activity.IsAllDay;
            StartHourTimePicker.Time = activity.StartHour;
            if (activity.StartDate != null)
                StartDatePicker.Date = activity.StartDate;
            if (activity.EndDate != null)
                EndDatePicker.Date = activity.EndDate;
            if (activity.Estimation != null)
                EstimateComboBox.SelectedIndex = (int)activity.Estimation;
            if (activity.Context != null)
                ContextComboBox.SelectedIndex = (int)activity.Context;
            if (activity.Project != null)
                ProjectComboBox.SelectedIndex = (int)activity.Project;
        }

        private void IsAllDayToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (IsAllDayToggleSwitch.IsOn)
            {
                StartHourRelativePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                StartHourRelativePanel.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem priorSelectedItem = (ComboBoxItem)PriorComboBox.SelectedItem;
            ComboBoxItem estimateSelectedItem = (ComboBoxItem)EstimateComboBox.SelectedItem;
            ComboBoxItem contextSelectedItem = (ComboBoxItem)ContextComboBox.SelectedItem;
            ComboBoxItem projectSelectedItem = (ComboBoxItem)ProjectComboBox.SelectedItem;

            string _title = TitleTextBox.Text;
            string _description = DescriptionTextBox.Text;
            string _priority = (priorSelectedItem != null) ? priorSelectedItem.Content.ToString() : "";
            bool _isAllDay = IsAllDayToggleSwitch.IsOn;
            string _startHour = StartHourTimePicker.Time.ToString();
            DateTimeOffset? _startDate = StartDatePicker.Date;
            DateTimeOffset? _endDate = EndDatePicker.Date;
            string _estimation = (estimateSelectedItem != null) ? estimateSelectedItem.Content.ToString() : ""; ;
            string _context = (contextSelectedItem != null) ? contextSelectedItem.Content.ToString() : ""; ;
            string _project = (projectSelectedItem != null) ? projectSelectedItem.Content.ToString() : ""; ;

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
