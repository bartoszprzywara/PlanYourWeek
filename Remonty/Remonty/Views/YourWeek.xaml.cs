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
            this.InitializeComponent();
            PlanYourWeek();
        }

        private void PlanYourWeek()
        {
            FillPlannedDay1();
            FillPlannedDay2WithActivities();
            FillPlannedDay3WithPlaceholders();

            /*
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
            {
                DateTimeOffset TodayTemp = (DateTimeOffset.Now).Date + new TimeSpan(0, 0, 0);

                ActivitiesForDay1 = new ObservableCollection<Activity>(conn.Query<Activity>("SELECT * FROM Activity WHERE IsDone = 0 AND List = 'Zaplanowane' AND StartDate = '" + TodayTemp.UtcTicks + "'").ToList());
                ActivitiesForDay2 = new ObservableCollection<Activity>(conn.Query<Activity>("SELECT * FROM Activity WHERE IsDone = 0 AND IsPlaceholder = 0").ToList());
                ActivitiesForDay3 = new ObservableCollection<Activity>(conn.Query<Activity>("SELECT * FROM Activity WHERE IsDone = 0 AND IsPlaceholder = 1").ToList());
            }
            */
        }

        private ObservableCollection<PlannedActivity> PlannedDay1 = new ObservableCollection<PlannedActivity>();
        private ObservableCollection<PlannedActivity> PlannedDay2 = new ObservableCollection<PlannedActivity>();
        private ObservableCollection<PlannedActivity> PlannedDay3 = new ObservableCollection<PlannedActivity>();

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedActivity = (PlannedActivity)e.ClickedItem;

            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(AddEditActivity), selectedActivity.ProposedActivity);
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            int ItemId = (int)((FrameworkElement)e.OriginalSource).DataContext;

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                conn.Execute("UPDATE Activity SET IsDone = 1 WHERE Id = " + ItemId);

            int i = 0;
            while (PlannedDay1[i].ProposedActivity.Id != ItemId)
                i++;
            PlannedDay1.RemoveAt(i); // not necessary but for animation
            PlannedDay1[i] = new PlannedActivity(i + 8);
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
            if (day == 1) return "Pon";
            if (day == 2) return "Wto";
            if (day == 3) return "Śro";
            if (day == 4) return "Czw";
            if (day == 5) return "Pią";
            if (day == 6) return "Sob";
            else return "Nie";
        }

        #endregion

        private void FillPlannedDay1()
        {
            for (int i = 8; i <= 22; i++)
                PlannedDay1.Add(new PlannedActivity(i));

            ObservableCollection<Activity> tempActivityList;
            DateTimeOffset TodayTemp = (DateTimeOffset.Now).Date + new TimeSpan(0, 0, 0);

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                tempActivityList = new ObservableCollection<Activity>(conn.Query<Activity>(
                    "SELECT * FROM Activity " +
                    "WHERE IsDone = 0 " +
                    "AND List = 'Zaplanowane' " +
                    "AND StartHour IS NOT NULL"
                    //AND StartDate = '" + TodayTemp.UtcTicks + "'"
                    ).ToList());

            foreach (var act in tempActivityList)
            {
                int actId = act.StartHour.Value.Hours;
                for (int i = 0; i < PlannedDay1.Count; i++)
                {
                    if (PlannedDay1[i].Id == actId)
                    {
                        while (PlannedDay1[i].ProposedActivity.IsPlaceholder != true && i < PlannedDay1.Count - 1)
                            i++;
                        if (PlannedDay1[i].ProposedActivity.IsPlaceholder == true)
                            PlannedDay1[i].ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(act.Id);
                        else if (PlannedDay1[i].Id + 1 < 24)
                            PlannedDay1.Add(new PlannedActivity(PlannedDay1[i].Id + 1, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
                        else
                            PlannedDay1.Add(new PlannedActivity(PlannedDay1[i].Id + 1 - 24, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
                        break;
                    }
                }
            }
            // TODO: co z godzina np. 7:00 albo 5:00 ?
            // TODO: poprawic usuwanie act z list kiedy klikniecie na Done

            //var item = PlannedDay1.FirstOrDefault(i => i.Id == 12);
            //item.ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(8);
        }

        private void FillPlannedDay2WithActivities()
        {
            for (int i = 11; i <= 17; i++)
                PlannedDay2.Add(new PlannedActivity(i) { Id = i, StartHour = new TimeSpan(i, 0, 0), ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(i) });
        }

        private void FillPlannedDay3WithPlaceholders()
        {
            for (int i = 11; i <= 17; i++)
                PlannedDay3.Add(new PlannedActivity(i));
        }
    }
}
