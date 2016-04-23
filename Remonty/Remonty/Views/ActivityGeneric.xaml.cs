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
    public sealed partial class ActivityGeneric : Page
    {
        public ActivityGeneric()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;
            listType = e.Parameter.ToString();

            if (listType.Contains("Szukaj"))
            {
                SearchValueTextBlock.Visibility = Visibility.Visible;
                SearchValueTextBlock.Text = "Wyniki dla: " + App.LastSearchValue;
                string searchValue = App.LastSearchValue.ToLower();

                using (LocalDatabaseHelper.conn.Lock())
                    listofActivities = new ObservableCollection<Activity>(LocalDatabaseHelper.conn.Query<Activity>("SELECT * FROM Activity WHERE IsDone = 0 ORDER BY Id DESC").Where(
                        v => v.Title.ToLower().Contains(searchValue) || v.Description.ToLower().Contains(searchValue)).ToList());
            }
            else if (listType == "Zrobione")
                using (LocalDatabaseHelper.conn.Lock())
                    listofActivities = new ObservableCollection<Activity>(LocalDatabaseHelper.conn.Query<Activity>("SELECT * FROM Activity WHERE IsDone = 1 ORDER BY Id DESC").ToList());
            else
                using (LocalDatabaseHelper.conn.Lock())
                    listofActivities = new ObservableCollection<Activity>(LocalDatabaseHelper.conn.Query<Activity>("SELECT * FROM Activity WHERE IsDone = 0 ORDER BY Id DESC").Where(
                        v => v.List == listType).ToList());

            if (App.PlannedWeekNeedsToBeReloaded)
            {
                App.ReloadPlannedWeekTask?.Wait();
                App.ReloadPlannedWeekTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    (new YourWeekPlanningHelper()).GetPlannedWeek();
                    App.PlannedWeekNeedsToBeReloaded = false;
                });
            }
        }

        private string listType;
        private ObservableCollection<Activity> listofActivities;

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedActivity = (Activity)e.ClickedItem;

            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(AddEditActivity), selectedActivity);
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            int ItemId = (int)((FrameworkElement)e.OriginalSource).DataContext;

            if (listType != "Zrobione")
                LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsDone = 1 WHERE Id = " + ItemId);
            else
                LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsDone = 0 WHERE Id = " + ItemId);

            int i = 0;
            while (listofActivities[i].Id != ItemId)
                i++;

            listofActivities.RemoveAt(i);

            App.ReloadPlannedWeekTask?.Wait();
            App.ReloadPlannedWeekTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                (new YourWeekPlanningHelper()).GetPlannedWeek();
            });
        }
    }
}
