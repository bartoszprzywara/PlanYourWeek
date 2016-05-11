using Remonty.Helpers;
using Remonty.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Appointments;
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
            InitializeClearButtons();
            AddActivityModeSetControls();
        }

        private Activity activity;
        private Context context;
        private ObservableCollection<Context> listOfContexts;
        private Project project;
        private ObservableCollection<Project> listOfProjects;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                BackButton.Visibility = Visibility.Collapsed;

            if (e.Parameter == null) return;
            activity = e.Parameter as Activity;

            TitleTextBlock.Text = "Edytuj zadanie";

            EditActivityModeSetControls();
            LoadActivityValuesIntoControls();

            if (activity.IsDone)
            {
                DoneButton.Visibility = Visibility.Collapsed;
                UnDoneButton.Visibility = Visibility.Visible;
            }

            /* AddToCalendar
            else if (activity.List == "Zaplanowane")
                AddToCalendarButton.Visibility = Visibility.Visible; */
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        private async void UnDoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                await (new MessageDialog("Zadanie musi mieć chociaż nazwę", "Nie da rady")).ShowAsync();
            else if (!AreStartAndEndDatesCorrect())
                await (new MessageDialog("Zadanie powinno kończyć się później, niż się zaczyna", "Nie da rady")).ShowAsync();
            else
            {
                LocalDatabaseHelper.UpdateActivity(activity.Id, LoadActivityValuesFromControls());
                LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsDone = 0 WHERE Id = " + activity.Id);
                ToastNotificationHelper.AddNotification(LocalDatabaseHelper.ReadItem<Activity>(activity.Id));
                App.PlannedWeekNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        private async void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                await (new MessageDialog("Zadanie musi mieć chociaż nazwę", "Nie da rady")).ShowAsync();
            else if (!AreStartAndEndDatesCorrect())
                await (new MessageDialog("Zadanie powinno kończyć się później, niż się zaczyna", "Nie da rady")).ShowAsync();
            else
            {
                LocalDatabaseHelper.UpdateActivity(activity.Id, LoadActivityValuesFromControls());
                LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsDone = 1 WHERE Id = " + activity.Id);
                ToastNotificationHelper.RemoveNotification(activity);
                App.PlannedWeekNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        async private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Na pewno chcesz bezpowrotnie usunąć to zadanie?", "Na pewno?");
            dialog.Commands.Add(new UICommand("Tak") { Id = 0 });
            dialog.Commands.Add(new UICommand("Nie") { Id = 1 });
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();

            if ((int)result.Id == 0)
            {
                LocalDatabaseHelper.ExecuteQuery("DELETE FROM Activity WHERE Id = " + activity.Id);
                ToastNotificationHelper.RemoveNotification(activity);
                App.PlannedWeekNeedsToBeReloaded = true;
                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        async private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                await (new MessageDialog("Zadanie musi mieć chociaż nazwę", "Nie da rady")).ShowAsync();
            else if (!AreStartAndEndDatesCorrect())
                await (new MessageDialog("Zadanie powinno kończyć się później, niż się zaczyna", "Nie da rady")).ShowAsync();
            else
            {
                LocalDatabaseHelper.InsertItem<Activity>(LoadActivityValuesFromControls());
                ToastNotificationHelper.AddNotification(LocalDatabaseHelper.ReadLastItem<Activity>());
                App.PlannedWeekNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        async private void SaveExistingButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                await (new MessageDialog("Zadanie musi mieć chociaż nazwę", "Nie da rady")).ShowAsync();
            else if (!AreStartAndEndDatesCorrect())
                await (new MessageDialog("Zadanie powinno kończyć się później, niż się zaczyna", "Nie da rady")).ShowAsync();
            else
            {
                LocalDatabaseHelper.UpdateActivity(activity.Id, LoadActivityValuesFromControls());
                ToastNotificationHelper.AddNotification(LocalDatabaseHelper.ReadItem<Activity>(activity.Id));
                App.PlannedWeekNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        private void IsAllDayToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            SetStartAndEndHourVisibility();
        }

        private void ListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((string)ListComboBox.SelectedItem == "Zaplanowane")
            {
                StartDateRelativePanel.Visibility = Visibility.Visible;
                if (StartDatePicker.Date == null)
                    StartDatePicker.Date = DateTimeOffset.Now;
                /* AddToCalendar
                if (activity != null && !activity.IsDone)
                    AddToCalendarButton.Visibility = Visibility.Visible; */
                ShowStartDateRelativePanelStoryboard.Begin();
            }
            else
            {
                StartDateRelativePanel.Visibility = Visibility.Collapsed;
                StartHourRelativePanel.Visibility = Visibility.Collapsed;
                /* AddToCalendar
                AddToCalendarButton.Visibility = Visibility.Collapsed; */
            }
            SetStartAndEndHourVisibility();
        }

        private void SetStartAndEndHourVisibility()
        {
            if (IsAllDayToggleSwitch.IsOn)
            {
                StartHourRelativePanel.Visibility = Visibility.Collapsed;
                EndHourRelativePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (ListComboBox.SelectedItem.ToString() == "Zaplanowane" && StartHourRelativePanel.Visibility != Visibility.Visible)
                {
                    StartHourRelativePanel.Visibility = Visibility.Visible;
                    ShowStartHourRelativePanelStoryboard.Begin();
                    if (activity == null || (activity != null && activity.StartHour == null))
                        StartHourTimePicker.Time = new TimeSpan(0, 0, 0);
                }
                if (EndHourRelativePanel.Visibility != Visibility.Visible)
                {
                    EndHourRelativePanel.Visibility = Visibility.Visible;
                    ShowEndHourRelativePanelStoryboard.Begin();
                }
                if (activity == null)
                    EndHourTimePicker.Time = new TimeSpan(0, 0, 0);
            }
        }

        #region Helpers

        private void SetUpPageAnimation()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;
        }

        private void InitializeComboBoxes()
        {
            ListComboBox.ItemsSource = new string[] { "Nowe", "Zaplanowane", "Najbliższe", "Kiedyś", "Oddelegowane" };
            PriorityComboBox.ItemsSource = LocalDatabaseHelper.ReadNamesFromTable<Priority>();
            EstimationComboBox.ItemsSource = LocalDatabaseHelper.ReadNamesFromTable<Estimation>();
            listOfContexts = LocalDatabaseHelper.ReadAllItemsFromTable<Context>();
            listOfProjects = LocalDatabaseHelper.ReadAllItemsFromTable<Project>();
        }

        private void InitializeClearButtons()
        {
            EndDateClearButton.Visibility = Visibility.Collapsed;
            EstimationClearButton.Visibility = Visibility.Collapsed;
            ContextClearButton.Visibility = Visibility.Collapsed;
            ProjectClearButton.Visibility = Visibility.Collapsed;
        }

        private bool AreStartAndEndDatesCorrect()
        {
            if (ListComboBox.SelectedItem.ToString() == "Zaplanowane" && EndDatePicker.Date != null)
            {
                DateTimeOffset tempStartDate = ((DateTimeOffset)StartDatePicker.Date).Date + new TimeSpan(0, 0, 0);
                DateTimeOffset tempEndDate = ((DateTimeOffset)EndDatePicker.Date).Date + new TimeSpan(0, 0, 0);
                if (!IsAllDayToggleSwitch.IsOn)
                {
                    tempStartDate += StartHourTimePicker.Time;
                    tempEndDate += EndHourTimePicker.Time;
                    return tempStartDate < tempEndDate;
                }
                return tempStartDate <= tempEndDate;
            }
            return true;
        }

        private void AddActivityModeSetControls()
        {
            PriorityComboBox.SelectedIndex = 1;
            ListComboBox.SelectedItem = "Zaplanowane";
            IsAllDayToggleSwitch.IsOn = true;
            StartDatePicker.Date = DateTimeOffset.Now;

            StartDateRelativePanel.Visibility = Visibility.Visible;
            StartHourRelativePanel.Visibility = Visibility.Collapsed;
            EndDateRelativePanel.Visibility = Visibility.Visible;
            EndHourRelativePanel.Visibility = Visibility.Collapsed;

            DebugButtonSetVisibility();
        }

        private void EditActivityModeSetControls()
        {
            DoneButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            SaveButton.Click -= new RoutedEventHandler(SaveButton_Click);
            SaveButton.Click += new RoutedEventHandler(SaveExistingButton_Click);

            DebugButtonSetVisibility();
        }

        private void LoadActivityValuesIntoControls()
        {
            TitleTextBox.Text = activity.Title;
            DescriptionTextBox.Text = activity.Description;
            PriorityComboBox.SelectedIndex = activity.PriorityId - 1;
            IsAllDayToggleSwitch.IsOn = activity.IsAllDay;
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
            DateTimeOffset? StartDateTemp = null;
            DateTimeOffset? EndDateTemp = null;
            if (StartDatePicker.Date != null)
                StartDateTemp = ((DateTimeOffset)StartDatePicker.Date).Date + new TimeSpan(0, 0, 0);
            if (EndDatePicker.Date != null)
                EndDateTemp = ((DateTimeOffset)EndDatePicker.Date).Date + new TimeSpan(0, 0, 0);

            return new Activity(
                    TitleTextBox.Text,
                    DescriptionTextBox.Text,
                    PriorityComboBox.SelectedIndex + 1,
                    IsAllDayToggleSwitch.IsOn,
                    ListComboBox.SelectedItem.ToString(),
                    StartDateTemp,
                    StartHourTimePicker.Time,
                    EndDateTemp,
                    EndHourTimePicker.Time,
                    (EstimationComboBox.SelectedItem != null) ? EstimationComboBox.SelectedIndex + 1 : (int?)null,
                    (int?)ContextComboBox.SelectedValue,
                    (int?)ProjectComboBox.SelectedValue
                    );
        }
        #endregion

        #region Debug button

        private void DebugButtonSetVisibility()
        {
#if DEBUG
            DebugAreaStrackPanel.Visibility = Visibility.Visible;
#else
            DebugAreaStrackPanel.Visibility = Visibility.Collapsed;
#endif
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            Activity tempActivity = LoadActivityValuesFromControls();

            IdTextBlock.Text = (activity != null) ? "Id: " + activity.Id : "Id: -1";
            SummaryTextBlock.Text = "Tytuł: " + tempActivity.Title + "\n" +
                                "Opis: " + tempActivity.Description + "\n" +
                                "Prior: " + tempActivity.PriorityUI + "\t" +
                                "CzyCałyDzień: " + tempActivity.IsAllDay + "\n" +
                                "Lista: " + tempActivity.List + "\n" +
                                "Start: " + tempActivity.StartDate + "\n" +
                                "Godzina: " + tempActivity.StartHour + "\n" +
                                "Kuniec: " + tempActivity.EndDate + "\n" +
                                "Godzina: " + tempActivity.EndHour + "\n" +
                                "Estim: " + tempActivity.EstimationUI + "\t" +
                                "Kontekst: " + tempActivity.ContextUI + "\t" +
                                "Projekt: " + tempActivity.ProjectUI + "\n" +
                                "IsDone: " + (activity?.IsDone ?? false);
        }
        #endregion

        #region Clear controls values

        private void StartDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            // prevent from setting null date
            if (StartDatePicker.Date == null)
                StartDatePicker.Date = DateTimeOffset.Now;
        }

        private void EndDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            EndDateClearButton.Visibility = Visibility.Visible;

            if (EndDatePicker.Date == null)
            {
                EndHourTimePicker.Time = new TimeSpan(0, 0, 0);
                EndDateClearButton.Visibility = Visibility.Collapsed;
            }
        }

        private void EndDateClearButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EndDatePicker.Date = null;
            EndHourTimePicker.Time = new TimeSpan(0, 0, 0);
            EndDateClearButton.Visibility = Visibility.Collapsed;
        }

        private void EstimationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EstimationClearButton.Visibility = Visibility.Visible;
        }

        private void EstimationClearButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EstimationComboBox.SelectedItem = null;
            EstimationClearButton.Visibility = Visibility.Collapsed;
        }

        private void ContextComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ContextClearButton.Visibility = Visibility.Visible;
        }

        private void ContextClearButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContextComboBox.SelectedItem = null;
            ContextClearButton.Visibility = Visibility.Collapsed;
        }

        private void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProjectClearButton.Visibility = Visibility.Visible;
        }

        private void ProjectClearButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ProjectComboBox.SelectedItem = null;
            ProjectClearButton.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Context and Project proposing
        private void TitleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((activity != null && !string.IsNullOrWhiteSpace(TitleTextBox.Text) && activity.Description != TitleTextBox.Text) ||
                (activity == null && !string.IsNullOrWhiteSpace(TitleTextBox.Text)))
            {
                ProposeContext();
                ProposeProject();
            }
        }

        private void DescriptionTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((activity != null && !string.IsNullOrWhiteSpace(DescriptionTextBox.Text) && activity.Description != DescriptionTextBox.Text) ||
                (activity == null && !string.IsNullOrWhiteSpace(DescriptionTextBox.Text)))
            {
                ProposeContext();
                ProposeProject();
            }
        }

        private void ProposeContext()
        {
            List<Context> allItems = LocalDatabaseHelper.conn.Table<Context>().ToList();
            List<string> allItemsSplit = new List<string>();
            List<string> tempItemsSplit;
            List<string> foundItem;

            foreach (var item in allItems)
            {
                tempItemsSplit = new List<string>(item.Name.Split(' '));
                foreach (var tempItem in tempItemsSplit)
                {
                    if (tempItem.Length > 3)
                        allItemsSplit.Add(tempItem);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                foundItem = allItemsSplit.Where(
                    v => ((DescriptionTextBox.Text.ToLower()).Contains(((v.Length > (i + 2) && i > 0) ? v.Remove(v.Length - i) : v).ToLower())
                    || (TitleTextBox.Text.ToLower()).Contains(((v.Length > (i + 2) && i > 0) ? v.Remove(v.Length - i) : v).ToLower()))).ToList();

                if (foundItem.Count > 0)
                {
                    foreach (var item in allItems)
                        if (item.Name.Contains(foundItem[0]))
                            ContextComboBox.SelectedValue = item.Id;
                    break;
                }
            }
        }

        private void ProposeProject()
        {
            List<Project> allItems = LocalDatabaseHelper.conn.Table<Project>().ToList();
            List<string> allItemsSplit = new List<string>();
            List<string> tempItemsSplit;
            List<string> foundItem;

            foreach (var item in allItems)
            {
                tempItemsSplit = new List<string>(item.Name.Split(' '));
                foreach (var tempItem in tempItemsSplit)
                {
                    if (tempItem.Length > 3)
                        allItemsSplit.Add(tempItem);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                foundItem = allItemsSplit.Where(
                    v => ((DescriptionTextBox.Text.ToLower()).Contains(((v.Length > (i + 2) && i > 0) ? v.Remove(v.Length - i) : v).ToLower())
                    || (TitleTextBox.Text.ToLower()).Contains(((v.Length > (i + 2) && i > 0) ? v.Remove(v.Length - i) : v).ToLower()))).ToList();

                if (foundItem.Count > 0)
                {
                    foreach (var item in allItems)
                        if (item.Name.Contains(foundItem[0]))
                            ProjectComboBox.SelectedValue = item.Id;
                    break;
                }
            }
        }
        #endregion

        #region Round timepicker values
        private void StartHourTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            int interval = 30; //30 min
            int minutes = e.NewTime.Minutes;
            if (minutes % interval != 0)
                StartHourTimePicker.Time += new TimeSpan(0, interval - (minutes % interval), 0);
        }

        private void EndHourTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            int interval = 30; //30 min
            int minutes = e.NewTime.Minutes;
            if (minutes % interval != 0)
                EndHourTimePicker.Time += new TimeSpan(0, interval - (minutes % interval), 0);
        }
        #endregion
    }
}
