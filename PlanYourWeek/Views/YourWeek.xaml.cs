using PlanYourWeek.Helpers;
using PlanYourWeek.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace PlanYourWeek
{
    public sealed partial class YourWeek : Page
    {
        public YourWeek()
        {
            App.ReloadPlannedWeekTask?.Wait();
            this.InitializeComponent();
            SetWeekDayNames();
            GetPlannedWeek();
            SetHoursColorAndVisibility();
            SetYourWeekPivot();
            YourWeekPivot.SelectedIndex = App.LastPivotItem;
        }

        // List of planned days which goes to UI
        private ObservableCollection<PlannedActivity>[] PlannedDay;
        // Total and used hours in the plan
        private double[] TotalHours;
        private double[] UsedHours;
        private double[] TotalWorkingHours;
        private double[] UsedWorkingHours;
        // Color of total and used hours
        private string[] HoursColor = new string[7];
        private string[] HoursWorkingColor = new string[7];
        private Visibility[] IsVisible = new Visibility[7];

        private void GetPlannedWeek()
        {
            if (App.PlannedWeekNeedsToBeReloaded)
            {
                (new YourWeekPlanningHelper()).GetPlannedWeek();
                CheckIfPlanIsOverfilled();
                App.PlannedWeekNeedsToBeReloaded = false;
            }

            PlannedDay = App.FinalPlannedWeekItems.PlannedWeek;
            TotalHours = App.FinalPlannedWeekItems.TotalHours;
            UsedHours = App.FinalPlannedWeekItems.UsedHours;
            TotalWorkingHours = App.FinalPlannedWeekItems.TotalWorkingHours;
            UsedWorkingHours = App.FinalPlannedWeekItems.UsedWorkingHours;
        }

        private async void CheckIfPlanIsOverfilled()
        {
            for (int i = 0; i < 7; i++)
            {
                if (App.FinalPlannedWeekItems.UsedHours[i] > App.FinalPlannedWeekItems.TotalHours[i])
                {
                    var messageTitle = LocalizedStrings.GetString("YourWeek_Alert_LimitExceeded_Title/Text");
                    var messageContent = LocalizedStrings.GetString("YourWeek_Alert_LimitExceeded_Content/Text");
                    await (new MessageDialog(messageContent + " (" + Days[i] + ")", messageTitle)).ShowAsync();
                    break;
                }
            }
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
            ToastNotificationHelper.RemoveNotification(LocalDatabaseHelper.ReadItem<Activity>(ItemId));

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

            App.ReloadPlannedWeekTask?.Wait();
            App.ReloadPlannedWeekTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                tempPlannedWeek.GetPlannedWeek();
            });
        }

        #region Hours Color
        private void SetHoursColorAndVisibility()
        {
            for (int i = 0; i < 7; i++)
            {
                HoursColor[i] = "DarkGreen";
                if (UsedHours[i] > TotalHours[i] - 1.5 && UsedHours[i] != 0)
                    HoursColor[i] = "Orange";
                if (UsedHours[i] > TotalHours[i])
                    HoursColor[i] = "Red";

                HoursWorkingColor[i] = "DarkGreen";
                if (UsedWorkingHours[i] > TotalWorkingHours[i] - 1.5 && UsedWorkingHours[i] != 0)
                    HoursWorkingColor[i] = "Orange";
                if (UsedWorkingHours[i] > TotalWorkingHours[i])
                    HoursWorkingColor[i] = "Red";

                IsVisible[i] = (TotalWorkingHours[i] == 0) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        #endregion

        #region Pivot Headers
        private int today = (int)DateTime.Today.DayOfWeek;
        public string[] Days = new string[7];

        private void SetWeekDayNames()
        {
            Days[0] = LocalizedStrings.GetString("Global_WeekDays_Today/Content");
            Days[1] = LocalizedStrings.GetString("Global_WeekDays_Tomorrow/Content");
            for (int i = 2; i < 7; i++)
                Days[i] = GetDayOfWeek((today + i) % 7);
        }

        private string GetDayOfWeek(int day)
        {
            if (day == 1) return LocalizedStrings.GetString("Global_WeekDaysShort_Monday/Content");
            if (day == 2) return LocalizedStrings.GetString("Global_WeekDaysShort_Tuesday/Content");
            if (day == 3) return LocalizedStrings.GetString("Global_WeekDaysShort_Wednesday/Content");
            if (day == 4) return LocalizedStrings.GetString("Global_WeekDaysShort_Thursday/Content");
            if (day == 5) return LocalizedStrings.GetString("Global_WeekDaysShort_Friday/Content");
            if (day == 6) return LocalizedStrings.GetString("Global_WeekDaysShort_Saturday/Content");
            else return LocalizedStrings.GetString("Global_WeekDaysShort_Sunday/Content");
        }
        #endregion

        #region Pivot Content
        private void SetYourWeekPivot()
        {
            List<PivotModel> items = new List<PivotModel>();

            for (int i = 0; i < 7; i++)
            {
                items.Add(new PivotModel()
                {
                    Header = Days[i],
                    ListViewSource = PlannedDay[i],
                    TotalHours = TotalHours[i],
                    UsedHours = UsedHours[i],
                    TotalWorkingHours = TotalWorkingHours[i],
                    UsedWorkingHours = UsedWorkingHours[i],
                    HoursColor = HoursColor[i],
                    HoursWorkingColor = HoursWorkingColor[i],
                    IsVisible = IsVisible[i]
                });
            }

            YourWeekPivot.ItemsSource = items;
        }
    }
    class PivotModel
    {
        public string Header { get; set; }
        public ObservableCollection<PlannedActivity> ListViewSource { get; set; }
        public double TotalHours { get; set; }
        public double UsedHours { get; set; }
        public double TotalWorkingHours { get; set; }
        public double UsedWorkingHours { get; set; }
        public string HoursColor { get; set; }
        public string HoursWorkingColor { get; set; }
        public Visibility IsVisible { get; set; }
    }
    #endregion
}
