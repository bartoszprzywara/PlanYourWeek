using Remonty.Helpers;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Remonty
{
    public sealed partial class AddEditActivity : Page
    {
        public AddEditActivity()
        {
            this.InitializeComponent();
            SetUpPageAnimation();
            InitializeComboBoxes();

            IsAllDayToggleSwitch.IsOn = true;
            StartHourRelativePanel.Visibility = Visibility.Collapsed;
            EndHourRelativePanel.Visibility = Visibility.Collapsed;
        }

        public void SetUpPageAnimation()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;
        }

        private Activity activity;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;
            activity = e.Parameter as Activity;

            TitleTextBlock.Text = (activity.Title != null) ? activity.Title : "Twoje zadanie";

            DoneButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            SaveButton.Click -= new RoutedEventHandler(SaveButton_Click);
            SaveButton.Click += new RoutedEventHandler(SaveExistingButton_Click);

            LoadActivityValuesIntoControls();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        async private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Na pewno chcesz bezpowrotnie usunąć to zadanie?", "Na pewno?");
            dialog.Commands.Add(new UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new UICommand("No") { Id = 1 });
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();

            if ((int)result.Id == 0)
            {
                LocalDatabaseHelper.DeleteItem<Activity>(activity.Id);
                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        async private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (TitleTextBox.Text == "")
            {
                var dialog = new MessageDialog("Zadanie musi mieć chociaż nazwę", "Nie da rady");
                await dialog.ShowAsync();
            }
            else {
                Activity tempActivity = LoadActivityValuesFromControls();
                LocalDatabaseHelper.InsertItem<Activity>(tempActivity);

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        async private void SaveExistingButton_Click(object sender, RoutedEventArgs e)
        {
            if (TitleTextBox.Text == "")
            {
                var dialog = new MessageDialog("Zadanie musi mieć chociaż nazwę");
                await dialog.ShowAsync();
            }
            else {
                Activity tempActivity = LoadActivityValuesFromControls();
                LocalDatabaseHelper.UpdateActivity(activity.Id, tempActivity);

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        private void IsAllDayToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            // TODO: Implement animation of showing and hiding time picker
            if (IsAllDayToggleSwitch.IsOn)
            {
                StartHourRelativePanel.Visibility = Visibility.Collapsed;
                EndHourRelativePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                StartHourRelativePanel.Visibility = Visibility.Visible;
                EndHourRelativePanel.Visibility = Visibility.Visible;
            }
        }

        private void InitializeComboBoxes()
        {
            PriorComboBox.ItemsSource = new string[] { "Niski", "Normalny", "Wysoki" };
            EstimateComboBox.ItemsSource = new string[] { "10 min", "20 min", "30 min", "1 godz", "2 godz", "3 godz", "4 godz", "6 godz", "10 godz" };
            ContextComboBox.ItemsSource = LocalDatabaseHelper.ReadNamesFromTable<Context>();
            ProjectComboBox.ItemsSource = LocalDatabaseHelper.ReadNamesFromTable<Project>();
        }

        private void LoadActivityValuesIntoControls()
        {
            if (activity.Title != null)
                TitleTextBox.Text = activity.Title;
            if (activity.Description != null)
                DescriptionTextBox.Text = activity.Description;
            PriorComboBox.SelectedItem = activity.Priority;
            if (activity.IsAllDay != null)
                IsAllDayToggleSwitch.IsOn = (bool)activity.IsAllDay;
            StartDatePicker.Date = activity.StartDate;
            if (activity.StartHour != null)
                StartHourTimePicker.Time = (TimeSpan)activity.StartHour;
            EndDatePicker.Date = activity.EndDate;
            if (activity.EndHour != null)
                EndHourTimePicker.Time = (TimeSpan)activity.EndHour;
            EstimateComboBox.SelectedItem = activity.Estimation;
            ContextComboBox.SelectedItem = activity.Context;
            ProjectComboBox.SelectedItem = activity.Project;
        }

        private Activity LoadActivityValuesFromControls()
        {
            return new Activity(
                    TitleTextBox.Text,
                    DescriptionTextBox.Text,
                    (PriorComboBox != null) ? (string)(PriorComboBox.SelectedItem) : null,
                    IsAllDayToggleSwitch.IsOn,
                    StartDatePicker.Date,
                    StartHourTimePicker.Time,
                    EndDatePicker.Date,
                    EndHourTimePicker.Time,
                    (EstimateComboBox != null) ? (string)(EstimateComboBox.SelectedItem) : null,
                    (ContextComboBox != null) ? (string)(ContextComboBox.SelectedItem) : null,
                    (ProjectComboBox != null) ? (string)(ProjectComboBox.SelectedItem) : null
                    );
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            Activity tempActivity = LoadActivityValuesFromControls();

            IdTextBlock.Text = (activity != null) ? "Id: " + activity.Id : "Id: -1";
            Podsumowanie.Text = "Tytuł: " + tempActivity.Title + "\n" +
                                "Opis: " + tempActivity.Description + "\n" +
                                "Prior: " + tempActivity.Priority + "\t\t" +
                                "CzyCałyDzień: " + tempActivity.IsAllDay + "\n" +
                                "Start: " + tempActivity.StartDate + "\t\t" +
                                "Godzina: " + tempActivity.StartHour + "\n" +
                                "Kuniec: " + tempActivity.EndDate + "\t\t" +
                                "Godzina: " + tempActivity.EndHour + "\n" +
                                "Estim: " + tempActivity.Estimation + "\t" +
                                "Kontekst: " + tempActivity.Context + "\t" +
                                "Projekt: " + tempActivity.Project;
        }
    }
}
