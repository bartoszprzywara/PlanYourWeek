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
            GetSettings();
            PlanYourWeek();
        }

        private void GetSettings()
        {
            ObservableCollection<Settings> savedSettings = LocalDatabaseHelper.ReadAllItemsFromTable<Settings>();

            StartHour = TimeSpan.Parse(savedSettings[0].Value).Hours;
            StartWork = TimeSpan.Parse(savedSettings[1].Value).Hours;
            EndWork = TimeSpan.Parse(savedSettings[2].Value).Hours;
            EndHour = TimeSpan.Parse(savedSettings[3].Value).Hours;
            WorkingHoursEnabled = bool.Parse(savedSettings[4].Value);

            if (!WorkingHoursEnabled)
            {
                StartWork = -1;
                EndWork = -1;
            }
        }

        private void PlanYourWeek()
        {
            FillPlannedDay1();
            TemporaryFillPlannedDay2WithActivities();
            TemporaryFillPlannedDay3WithPlaceholders();
        }

        bool WorkingHoursEnabled;
        int StartHour, StartWork, EndWork, EndHour;
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

            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsDone = 1 WHERE Id = " + ItemId);

            int i = 0;
            while (PlannedDay1[i].ProposedActivity.Id != ItemId)
                i++;
            PlannedDay1[i] = new PlannedActivity(i + StartHour);
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
            // szybka edycja itemu na liście:
            //var item = PlannedDay1.FirstOrDefault(i => i.Id == 12);
            //item.ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(8);


            // --------------- KROK 1 ---------------
            // w kroku nr 1 trzeba wypełnić plan dnia placeholderami

            // wypełnij plan dnia pustymi aktywnościami - placeholderami
            for (int i = StartHour; i <= EndHour; i++)
                PlannedDay1.Add(new PlannedActivity(i));


            // --------------- KROK 2 ---------------
            // w kroku nr 2 trzeba dodać dzisiejsze, niewykonane aktywności z ustaloną godziną

            // znajdź w localdb zaplanowane, ale niewykonane aktywności, które zaczynają się dzisiaj i mają określoną godzinę
            ObservableCollection<Activity> tempActivityList;
            DateTimeOffset TodayTemp = (DateTimeOffset.Now).Date + new TimeSpan(0, 0, 0);

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                tempActivityList = new ObservableCollection<Activity>(conn.Query<Activity>(
                    "SELECT * FROM Activity " +
                    "WHERE IsDone = 0 " +
                    "AND List = 'Zaplanowane' " +
                    "AND StartHour IS NOT NULL " +
                    "AND StartDate >= '" + TodayTemp.UtcTicks + "'" // TODO: pamiętać o usunięciu znaku '>'
                    ).ToList());

            // każdą taką znalezioną aktywność dodaj do planu dnia pod odpowiednią godziną
            foreach (var act in tempActivityList)
            {
                int actId = act.StartHour.Value.Hours;
                // jeśli aktywność zaczyna się później, niż koniec planu dnia - wydłuż plan dnia
                if (actId > EndHour)
                {
                    for (int i = EndHour + 1; i < actId; i++)
                        PlannedDay1.Add(new PlannedActivity(i));
                    EndHour = actId;
                    PlannedDay1.Add(new PlannedActivity(actId, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
                }
                // jeśli aktywność zaczyna się wcześniej, niż początek planu dnia - zacznij plan dnia wcześniej
                else if (actId < StartHour)
                {
                    for (int i = StartHour - 1; i > actId; i--)
                        PlannedDay1.Insert(0, new PlannedActivity(i));
                    StartHour = actId;
                    PlannedDay1.Insert(0, new PlannedActivity(actId, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
                }
                // jeśli aktywność zaczyna sie w godzinach planu dnia - dodaj ją do planu dnia
                else
                {
                    for (int i = 0; i < PlannedDay1.Count; i++)
                    {
                        if (PlannedDay1[i].Id == actId)
                        {
                            // dodaj aktywność pod odpowiednią godziną, tylko jeśli jest miejsce
                            // jeśli nie ma miejsca, to spróbuj wstawić aktywność godzinę później
                            while (PlannedDay1[i].ProposedActivity.IsPlaceholder != true && i < PlannedDay1.Count - 1)
                                i++;
                            if (PlannedDay1[i].ProposedActivity.IsPlaceholder == true)
                                PlannedDay1[i].ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(act.Id);
                            // jeśli aktywność zaczyna się później, niż koniec dnia - dodaj ją mimo wszystko
                            else if (PlannedDay1[i].Id + 1 < 24)
                                PlannedDay1.Add(new PlannedActivity(PlannedDay1[i].Id + 1, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
                            // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                            else
                                PlannedDay1.Add(new PlannedActivity(PlannedDay1[i].Id + 1 - 24, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
                            break;
                        }
                    }
                }
            }


            // --------------- KROK 3 ---------------
            // w kroku nr 3 trzeba dodać dzisiejsze, niewykonane aktywności bez godziny

            // znajdź w localdb zaplanowane, ale niewykonane aktywności, które zaczynają się dzisiaj, ale NIE MAJĄ określonej godziny
            // posortuj znalezione aktywności od tych z najwyższym priorytetem
            // TODO: zacząć od najkrótszych
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                tempActivityList = new ObservableCollection<Activity>(conn.Query<Activity>(
                    "SELECT * FROM Activity " +
                    "WHERE IsDone = 0 " +
                    "AND List = 'Zaplanowane' " +
                    "AND StartHour IS NULL " +
                    "AND StartDate >= '" + TodayTemp.UtcTicks + "'" + // TODO: pamiętać o usunięciu znaku '>'
                    "ORDER BY PriorityId DESC"
                    ).ToList());

            // każdą taką znalezioną aktywność dodaj do planu dnia pod pierwszą wolną godziną
            foreach (var act in tempActivityList)
            {
                // jeśli nie ma miejsca, to spróbuj wstawić aktywność godzinę później
                // ale nie wstawiaj aktywności jeśli wolne miejsce zawiera się w godzinach pracy
                int i = 0;
                while ((PlannedDay1[i].ProposedActivity.IsPlaceholder != true && i < PlannedDay1.Count - 1) || (PlannedDay1[i].Id >= StartWork && PlannedDay1[i].Id < EndWork))
                    i++;
                if (PlannedDay1[i].ProposedActivity.IsPlaceholder == true)
                    PlannedDay1[i].ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(act.Id);
                // jeśli aktywność zostanie zaproponowana później, niż koniec dnia - zaproponuj ją mimo wszystko
                else if (PlannedDay1[i].Id + 1 < 24)
                    PlannedDay1.Add(new PlannedActivity(PlannedDay1[i].Id + 1, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
                // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                else
                    PlannedDay1.Add(new PlannedActivity(PlannedDay1[i].Id + 1 - 24, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
            }


            // --------------- KROKI 4, 5, 6 ---------------
            // w kroku nr 4 trzeba dodać zaplanowane, ale niewykonane aktywności z poprzednich dni
            // w kroku nr 5 trzeba dodać niewykonane aktywności z listy "najbliższe"
            // w kroku nr 6 trzeba dodać niewykonane aktywności z listy "kiedyś"

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                tempActivityList = new ObservableCollection<Activity>(conn.Query<Activity>(
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND List = 'Zaplanowane' AND StartDate < '" + TodayTemp.UtcTicks +
                        "'ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND List = 'Najblizsze' " +
                        "ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND List = 'Kiedys' " +
                        "ORDER BY PriorityId DESC, EstimationId ASC)"
                    ).ToList());

            // każdą taką znalezioną aktywność dodaj do planu dnia pod pierwszą wolną godziną
            // ale nie wstawiaj aktywności jeśli wolne miejsce zawiera się w godzinach pracy
            foreach (var act in tempActivityList)
            {
                // jeśli nie ma miejsca, to spróbuj wstawić aktywność godzinę później
                int i = 0;
                while ((PlannedDay1[i].ProposedActivity.IsPlaceholder != true && i < PlannedDay1.Count - 1) || (PlannedDay1[i].Id >= StartWork && PlannedDay1[i].Id < EndWork))
                    i++;
                if (PlannedDay1[i].ProposedActivity.IsPlaceholder == true)
                {
                    // dodatkowo datę aktywności z poprzednich dni oznacz kolorem czerwonym
                    if (PlannedDay1[i].ProposedActivity.StartDate != null)
                        PlannedDay1[i].Color = "Red";
                    PlannedDay1[i].ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(act.Id);
                }
                // jeśli aktywność zostanie zaproponowana później, niż koniec dnia - NIE proponuj jej
            }

            // TODO: uwzglednić podane godziny pracy w ustawieniach

            // KROK 7: Pomiń listę "Nowe" - takie aktywności należy najpierw przejrzeć i umieścić na jakiejś liście
            // KROK 8: Pomiń listę "Oddelegowane" - ktoś takie aktywności musi zrobić za nas
        }

        private void TemporaryFillPlannedDay2WithActivities()
        {
            for (int i = 11; i <= 17; i++)
                PlannedDay2.Add(new PlannedActivity(i) { Id = i, StartHour = new TimeSpan(i, 0, 0), ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(i) });
        }

        private void TemporaryFillPlannedDay3WithPlaceholders()
        {
            for (int i = 11; i <= 17; i++)
                PlannedDay3.Add(new PlannedActivity(i));
        }
    }
}
