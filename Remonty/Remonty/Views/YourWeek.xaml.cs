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
    public sealed partial class YourWeek : Page
    {
        public YourWeek()
        {
            App.ReloadPlannedWeekTask?.Wait();
            this.InitializeComponent();
            GetPlannedWeek();
            YourWeekPivot.SelectedIndex = App.LastPivotItem;

            // TODO: assertion failed sqlite3 .net
        }

        // List of planned days which goes to UI
        private ObservableCollection<PlannedActivity>[] PlannedDay;

        private void GetPlannedWeek()
        {
            if (App.PlannedWeekNeedsToBeReloaded)
            {
                var tempObj = new YourWeekPlanningHelper();
                tempObj.GetPlannedWeek();
                App.PlannedWeekNeedsToBeReloaded = false;
            }
            PlannedDay = App.FinalPlannedWeek;
        }

        private void YourWeekPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.LastPivotItem = YourWeekPivot.SelectedIndex;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedActivity = (PlannedActivity)e.ClickedItem;

            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(AddEditActivity), selectedActivity.ProposedActivity);
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            int ItemId = (int)((FrameworkElement)e.OriginalSource).DataContext;

            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsDone = 1 WHERE Id = " + ItemId);

            int i = 0;
            int duration = 0;
            int currentPivotItem = YourWeekPivot.SelectedIndex;
            var tempPlannedWeek = new YourWeekPlanningHelper();

            while (PlannedDay[currentPivotItem][i].ProposedActivity?.Id != ItemId)
            {
                if (PlannedDay[currentPivotItem][i].ProposedActivity != null)
                    duration += (tempPlannedWeek.GetActivityDuration(PlannedDay[currentPivotItem][i].ProposedActivity) - 1);
                i++;
            }

            for (int j = i + 1; j < i + tempPlannedWeek.GetActivityDuration(PlannedDay[currentPivotItem][i].ProposedActivity); j++)
                PlannedDay[currentPivotItem].Insert(j, new PlannedActivity(j + PlannedDay[currentPivotItem][0].Id + duration));
            PlannedDay[currentPivotItem][i] = new PlannedActivity(i + PlannedDay[currentPivotItem][0].Id + duration);

            App.ReloadPlannedWeekTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                tempPlannedWeek.GetPlannedWeek();
            });
        }

        #region Pivot Headers

        private int today = (int)DateTime.Today.DayOfWeek;
        public string Day3 { get { return GetDayOfWeek((today + 2) % 7); } }
        public string Day4 { get { return GetDayOfWeek((today + 3) % 7); } }
        public string Day5 { get { return GetDayOfWeek((today + 4) % 7); } }
        public string Day6 { get { return GetDayOfWeek((today + 5) % 7); } }
        public string Day7 { get { return GetDayOfWeek((today + 6) % 7); } }

        private string GetDayOfWeek(int day)
        {
            if (day == 1) return "pon";
            if (day == 2) return "wto";
            if (day == 3) return "śro";
            if (day == 4) return "czw";
            if (day == 5) return "pią";
            if (day == 6) return "sob";
            else return "nie";
        }

        #endregion
    }
}
