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

        private void StartDayTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + StartDayTimePicker.Time.ToString() + "' WHERE Name = 'StartDay'");
            if (screenEntered)
            {
                AnimateStartDayStackPanelStoryboard.Begin();
                ReloadPlannedWeek();
            }
        }

        private void StartWorkingTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + StartWorkingTimePicker.Time.ToString() + "' WHERE Name = 'StartWorking'");
            if (screenEntered)
            {
                AnimateStartWorkingStackPanelStoryboard.Begin();
                ReloadPlannedWeek();
            }
        }

        private void EndWorkingTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + EndWorkingTimePicker.Time.ToString() + "' WHERE Name = 'EndWorking'");
            if (screenEntered)
            {
                AnimateEndWorkingStackPanelStoryboard.Begin();
                ReloadPlannedWeek();
            }
        }

        private void EndDayTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            LocalDatabaseHelper.ExecuteQuery("UPDATE Settings SET Value = '" + EndDayTimePicker.Time.ToString() + "' WHERE Name = 'EndDay'");
            if (screenEntered)
            {
                AnimateEndDayStackPanelStoryboard.Begin();
                ReloadPlannedWeek();
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
