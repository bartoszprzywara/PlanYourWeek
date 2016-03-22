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
    public sealed partial class YourWeek : Page
    {
        public YourWeek()
        {
            this.InitializeComponent();
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                ActivitiesForDay1 = new ObservableCollection<Activity>(conn.Query<Activity>("SELECT * FROM Activity WHERE IsDone = 0").ToList());
        }

        private ObservableCollection<Activity> ActivitiesForDay1;

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedActivity = (Activity)e.ClickedItem;

            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(AddEditActivity), selectedActivity);
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            int ItemId = (int)((FrameworkElement)e.OriginalSource).DataContext;

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                conn.Execute("UPDATE Activity SET IsDone = 1 WHERE Id = " + ItemId);

            int i = 0;
            while (ActivitiesForDay1[i].Id != ItemId)
                i++;
            ActivitiesForDay1.RemoveAt(i);
        }

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
    }
}
