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
            WorkingHoursEnabledToggleSwitch.IsOn = bool.Parse(settingsList[4].Value);
        }

        private bool screenEntered = false;
        private bool navigatingTimePicker = false;

        private void StartDayTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (screenEntered && !navigatingTimePicker)
            {
                navigatingTimePicker = true;

                if (e.NewTime.Minutes != 0)
                    StartDayTimePicker.Time -= new TimeSpan(0, e.NewTime.Minutes, 0);

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

                if (e.NewTime.Minutes != 0)
                    StartWorkingTimePicker.Time -= new TimeSpan(0, e.NewTime.Minutes, 0);

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

                if (e.NewTime.Minutes != 0)
                    EndWorkingTimePicker.Time -= new TimeSpan(0, e.NewTime.Minutes, 0);

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

                if (e.NewTime.Minutes != 0)
                    EndDayTimePicker.Time -= new TimeSpan(0, e.NewTime.Minutes, 0);

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

        private void WorkingHoursEnabledToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + WorkingHoursEnabledToggleSwitch.IsOn.ToString() + "' WHERE Name = 'WorkingHoursEnabled'");
            if (screenEntered)
            {
                AnimateWorkingHoursStackPanelStoryboard.Begin();
                ReloadPlannedWeek();
            }
        }

        private void ReloadPlannedWeek()
        {
            App.ReloadPlannedWeekTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                (new YourWeekPlanningHelper()).GetPlannedWeek();
                App.PlannedWeekNeedsToBeReloaded = false;
            });
        }
    }
}
