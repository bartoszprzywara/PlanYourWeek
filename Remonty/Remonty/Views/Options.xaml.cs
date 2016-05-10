using Remonty.Helpers;
using Remonty.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace Remonty
{
    public sealed partial class Options : Page
    {
        public Options()
        {
            this.InitializeComponent();
            LoadControlsFromDb();
            screenEntered = true;
        }

        private void LoadControlsFromDb()
        {
            ObservableCollection<Settings> settingsList = LocalDatabaseHelper.ReadAllItemsFromTable<Settings>();

            StartDayTimePicker.Time = TimeSpan.Parse(settingsList[0].Value);
            StartWorkingTimePicker.Time = TimeSpan.Parse(settingsList[1].Value);
            EndWorkingTimePicker.Time = TimeSpan.Parse(settingsList[2].Value);
            EndDayTimePicker.Time = TimeSpan.Parse(settingsList[3].Value);

            SunToggleButton.IsChecked = bool.Parse(settingsList[4].Value);
            MonToggleButton.IsChecked = bool.Parse(settingsList[5].Value);
            TueToggleButton.IsChecked = bool.Parse(settingsList[6].Value);
            WedToggleButton.IsChecked = bool.Parse(settingsList[7].Value);
            ThuToggleButton.IsChecked = bool.Parse(settingsList[8].Value);
            FriToggleButton.IsChecked = bool.Parse(settingsList[9].Value);
            SatToggleButton.IsChecked = bool.Parse(settingsList[10].Value);
        }

        int interval = 30; //30 min
        private bool screenEntered = false;
        private bool navigatingTimePicker = false;

        private void StartDayTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (screenEntered && !navigatingTimePicker)
            {
                navigatingTimePicker = true;

                if (e.NewTime.Minutes % interval != 0)
                    StartDayTimePicker.Time += new TimeSpan(0, interval - (e.NewTime.Minutes % interval), 0);

                if (StartDayTimePicker.Time > StartWorkingTimePicker.Time)
                {
                    InfoMessageTextBlock.Text = "Nie da się wstawać później, niż zaczęło się pracę";
                    InfoMessageGrid.Opacity = 1;
                    AnimateInfoMessageGridStoryboard.Begin();
                    StartDayTimePicker.Time = e.OldTime;
                }
                else
                {
                    LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + StartDayTimePicker.Time.ToString() + "' WHERE Name = 'StartDay'");
                    AnimateStartDayStackPanelStoryboard.Begin();
                    ReloadPlannedWeek();
                }

                navigatingTimePicker = false;
            }
        }

        private void StartWorkingTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (screenEntered && !navigatingTimePicker)
            {
                navigatingTimePicker = true;

                if (e.NewTime.Minutes % interval != 0)
                    StartWorkingTimePicker.Time += new TimeSpan(0, interval - (e.NewTime.Minutes % interval), 0);

                if (StartDayTimePicker.Time > StartWorkingTimePicker.Time)
                {
                    InfoMessageTextBlock.Text = "Nie da się zaczynać pracy wcześniej, niż się wstało";
                    InfoMessageGrid.Opacity = 1;
                    AnimateInfoMessageGridStoryboard.Begin();
                    StartWorkingTimePicker.Time = e.OldTime;
                }
                else if (StartWorkingTimePicker.Time > EndWorkingTimePicker.Time)
                {
                    InfoMessageTextBlock.Text = "Nie da się zaczynać pracy później, niż się ją kończy";
                    InfoMessageGrid.Opacity = 1;
                    AnimateInfoMessageGridStoryboard.Begin();
                    StartWorkingTimePicker.Time = e.OldTime;
                }
                else
                {
                    LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + StartWorkingTimePicker.Time.ToString() + "' WHERE Name = 'StartWorking'");
                    AnimateStartWorkingStackPanelStoryboard.Begin();
                    ReloadPlannedWeek();
                }

                navigatingTimePicker = false;
            }
        }

        private void EndWorkingTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (screenEntered && !navigatingTimePicker)
            {
                navigatingTimePicker = true;

                if (e.NewTime.Minutes % interval != 0)
                    EndWorkingTimePicker.Time += new TimeSpan(0, interval - (e.NewTime.Minutes % interval), 0);

                if (StartWorkingTimePicker.Time > EndWorkingTimePicker.Time)
                {
                    InfoMessageTextBlock.Text = "Nie da się kończyć pracy wcześniej, niż się ją zaczęło";
                    InfoMessageGrid.Opacity = 1;
                    AnimateInfoMessageGridStoryboard.Begin();
                    EndWorkingTimePicker.Time = e.OldTime;
                }
                else if (EndWorkingTimePicker.Time > EndDayTimePicker.Time)
                {
                    InfoMessageTextBlock.Text = "Nie da się kończyć pracy później, niż idzie się spać";
                    InfoMessageGrid.Opacity = 1;
                    AnimateInfoMessageGridStoryboard.Begin();
                    EndWorkingTimePicker.Time = e.OldTime;
                }
                else
                {
                    LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + EndWorkingTimePicker.Time.ToString() + "' WHERE Name = 'EndWorking'");
                    AnimateEndWorkingStackPanelStoryboard.Begin();
                    ReloadPlannedWeek();
                }

                navigatingTimePicker = false;
            }
        }

        private void EndDayTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (screenEntered && !navigatingTimePicker)
            {
                navigatingTimePicker = true;

                if (e.NewTime.Minutes % interval != 0)
                    EndDayTimePicker.Time += new TimeSpan(0, interval - (e.NewTime.Minutes % interval), 0);

                if (EndWorkingTimePicker.Time > EndDayTimePicker.Time &&
                    StartDayTimePicker.Time < EndDayTimePicker.Time)
                {
                    InfoMessageTextBlock.Text = "Nie da się iść spać wcześniej, niż kończy się pracę";
                    InfoMessageGrid.Opacity = 1;
                    AnimateInfoMessageGridStoryboard.Begin();
                    EndDayTimePicker.Time = e.OldTime;
                }
                else
                {
                    LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + EndDayTimePicker.Time.ToString() + "' WHERE Name = 'EndDay'");
                    AnimateEndDayStackPanelStoryboard.Begin();
                    ReloadPlannedWeek();
                }

                navigatingTimePicker = false;
            }
        }

        private void WeekdayToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;

            LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + button.IsChecked.ToString() + "' WHERE Name = 'Is" + button.DataContext + "Workday'");
            if (screenEntered)
            {
                AnimateWorkdaysStackPanelStoryboard.Begin();
                ReloadPlannedWeek();
            }
        }

        private void ReloadPlannedWeek()
        {
            App.ReloadPlannedWeekTask?.Wait();
            App.ReloadPlannedWeekTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                (new YourWeekPlanningHelper()).GetPlannedWeek();
                App.PlannedWeekNeedsToBeReloaded = false;
            });
        }
    }
}
