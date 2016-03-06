using Remonty.Model;
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;
            TitleTextBlock.Text = "Edytuj zadanie";
            Activity activity = e.Parameter as Activity;
            if (activity.Title != null)
                TitleTextBox.Text = activity.Title;
            if (activity.Description != null)
                DescriptionTextBox.Text = activity.Description;
            if (activity.Priority != null)
                PriorComboBox.SelectedIndex = (int)activity.Priority;
            IsAllDayToggleSwitch.IsOn = activity.IsAllDay;
            if (activity.Start != null)
                StartDatePicker.Date = activity.Start;
            if (activity.End != null)
                EndDatePicker.Date = activity.End;
            if (activity.Estimation != null)
                EstimateComboBox.SelectedIndex = (int)activity.Estimation;
            if (activity.Context != null)
                ContextComboBox.SelectedIndex = (int)activity.Context;
            if (activity.Project != null)
                ProjectComboBox.SelectedIndex = (int)activity.Project;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            displayResult();
        }

        private void displayResult()
        {
            ComboBoxItem priorSelectedItem = (ComboBoxItem)PriorComboBox.SelectedItem;
            ComboBoxItem estimateSelectedItem = (ComboBoxItem)EstimateComboBox.SelectedItem;
            ComboBoxItem contextSelectedItem = (ComboBoxItem)ContextComboBox.SelectedItem;
            ComboBoxItem projectSelectedItem = (ComboBoxItem)ProjectComboBox.SelectedItem;

            string _title = TitleTextBox.Text;
            string _description = DescriptionTextBox.Text;
            string _priority = (priorSelectedItem != null) ? priorSelectedItem.Content.ToString() : "";
            bool _isAllDay = IsAllDayToggleSwitch.IsOn;
            DateTimeOffset? _start = StartDatePicker.Date;
            DateTimeOffset? _end = EndDatePicker.Date;
            string _estimation = (estimateSelectedItem != null) ? estimateSelectedItem.Content.ToString() : ""; ;
            string _context = (contextSelectedItem != null) ? contextSelectedItem.Content.ToString() : ""; ;
            string _project = (projectSelectedItem != null) ? projectSelectedItem.Content.ToString() : ""; ;

            Podsumowanie.Text = "Tytuł: " + _title + "\t" +
                                "Opis: " + _description + "\n" +
                                "Prior: " + _priority + "\t" +
                                "CzyCałyDzień: " + _isAllDay + "\n" +
                                "Start: " + _start + "\t" +
                                "Kuniec: " + _end + "\n" +
                                "Estim: " + _estimation + "\t" +
                                "Kontekst: " + _context + "\t" +
                                "Projekt: " + _project;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
