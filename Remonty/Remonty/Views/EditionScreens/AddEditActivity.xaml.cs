using Remonty.Helpers;
using Remonty.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private void SetUpPageAnimation()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;
        }

        private Activity activity;
        private Context context;
        private ObservableCollection<Context> listOfContexts;
        private Project project;
        private ObservableCollection<Project> listOfProjects;

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

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                conn.Query<Activity>("UPDATE Activity SET IsDone = 1 WHERE Id = " + activity.Id);

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
            // TODO: przeanalizowac jak tu zrobic polskie znaki dla nazw list
            ListComboBox.ItemsSource = new string[] { "Nowe", "Zaplanowane", "Najblizsze", "Kiedys", "Oddelegowane" };
            PriorityComboBox.ItemsSource = LocalDatabaseHelper.ReadNamesFromTable<Priority>();
            EstimationComboBox.ItemsSource = LocalDatabaseHelper.ReadNamesFromTable<Estimation>();
            listOfContexts = LocalDatabaseHelper.ReadAllItemsFromTable<Context>();
            listOfProjects = LocalDatabaseHelper.ReadAllItemsFromTable<Project>();
        }

        private void LoadActivityValuesIntoControls()
        {
            if (activity.Title != null)
                TitleTextBox.Text = activity.Title;
            if (activity.Description != null)
                DescriptionTextBox.Text = activity.Description;
            if (activity.PriorityId != null)
                PriorityComboBox.SelectedIndex = (int)activity.PriorityId - 1;
            if (activity.IsAllDay != null)
                IsAllDayToggleSwitch.IsOn = (bool)activity.IsAllDay;
            ListComboBox.SelectedItem = activity.List;
            StartDatePicker.Date = activity.StartDate;
            if (activity.StartHour != null)
                StartHourTimePicker.Time = (TimeSpan)activity.StartHour;
            EndDatePicker.Date = activity.EndDate;
            if (activity.EndHour != null)
                EndHourTimePicker.Time = (TimeSpan)activity.EndHour;
            if (activity.EstimationId != null)
                EstimationComboBox.SelectedIndex = (int)activity.EstimationId - 1;
            if (activity.ContextId != null)
                context = listOfContexts[LocalDatabaseHelper.ReadItemIndex<Context>("Id", (int)activity.ContextId)];
            if (activity.ProjectId != null)
                project = listOfProjects[LocalDatabaseHelper.ReadItemIndex<Project>("Id", (int)activity.ProjectId)];
        }

        private Activity LoadActivityValuesFromControls()
        {
            return new Activity(
                    TitleTextBox.Text,
                    DescriptionTextBox.Text,
                    (PriorityComboBox.SelectedItem != null) ? PriorityComboBox.SelectedIndex + 1 : (int?)null,
                    IsAllDayToggleSwitch.IsOn,
                    (string)ListComboBox.SelectedItem,
                    StartDatePicker.Date,
                    StartHourTimePicker.Time,
                    EndDatePicker.Date,
                    EndHourTimePicker.Time,
                    (EstimationComboBox.SelectedItem != null) ? EstimationComboBox.SelectedIndex + 1 : (int?)null,
                    (ContextComboBox.SelectedItem != null) ? (int)ContextComboBox.SelectedValue : (int?)null,
                    (ProjectComboBox.SelectedItem != null) ? (int)ProjectComboBox.SelectedValue : (int?)null
                    );
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            Activity tempActivity = LoadActivityValuesFromControls();

            IdTextBlock.Text = (activity != null) ? "Id: " + activity.Id : "Id: -1";
            Podsumowanie.Text = "Tytuł: " + tempActivity.Title + "\n" +
                                "Opis: " + tempActivity.Description + "\n" +
                                "Prior: " + tempActivity.PriorityUI + "\t\t" +
                                "CzyCałyDzień: " + tempActivity.IsAllDay + "\n" +
                                "Lista: " + tempActivity.List + "\n" +
                                "Start: " + tempActivity.StartDate + "\t\t" +
                                "Godzina: " + tempActivity.StartHour + "\n" +
                                "Kuniec: " + tempActivity.EndDate + "\t\t" +
                                "Godzina: " + tempActivity.EndHour + "\n" +
                                "Estim: " + tempActivity.EstimationUI + "\t" +
                                "Kontekst: " + tempActivity.ContextUI + "\t" +
                                "Projekt: " + tempActivity.ProjectUI + "\n" +
                                "IsDone: " + ((activity != null) ? activity.IsDone : false);
        }
    }
}
