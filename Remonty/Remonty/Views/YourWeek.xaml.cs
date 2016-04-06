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
            EndHour = TimeSpan.Parse(savedSettings[3].Value).Hours - 1;
            if (EndHour < 0 || EndHour < StartHour)
                EndHour = 24 + EndHour;
            WorkingHoursEnabled = bool.Parse(savedSettings[4].Value);

            if (!WorkingHoursEnabled)
            {
                StartWork = -1;
                EndWork = -1;
            }
        }

        bool WorkingHoursEnabled;
        int StartHour, StartWork, EndWork, EndHour;
        private ObservableCollection<PlannedActivity> PlannedDay1 = new ObservableCollection<PlannedActivity>();

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

        private void PlanYourWeek()
        {
            // TODO: optymalizacja drugiego kroku (praktycznie robie 2 razy to samo teraz w dwóch foreach-ach
            // TODO: zająć się przyciskiem done na aktywności w końcu
            // TODO: zrobić ograniczenie (graniczna godzina), żeby aktywności nie dodawały się w nieskończoność
            // TODO: przemyśleć sprawę połówek godziny i startów aktywności o nierównej godzinie
            // TODO: zająć się pozostałymi dniami w planie tygodnia


            // --------------- KROK 1 ---------------
            // w kroku nr 1 trzeba wypełnić plan dnia placeholderami

            // wypełnij plan dnia pustymi aktywnościami - placeholderami
            for (int i = StartHour; i <= EndHour; i++)
                PlannedDay1.Add(new PlannedActivity(i));


            // --------------- KROK 2 ---------------
            // w kroku nr 2 trzeba dodać dzisiejsze, niewykonane aktywności z ustaloną godziną

            // znajdź w localdb zaplanowane, ale niewykonane aktywności, które zaczynają się dzisiaj i mają określoną godzinę
            // posortuj znalezione aktywności od tych z najwyższym priorytetem i od najkrótszej
            // na koniec posortuj je godziną rozpoczęcia od najpóźniejszej
            ObservableCollection<Activity> tempActivityList;
            ObservableCollection<Activity> tempUnhandledActivityList = new ObservableCollection<Activity>();
            DateTimeOffset TodayTemp = (DateTimeOffset.Now).Date + new TimeSpan(0, 0, 0);

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                tempActivityList = new ObservableCollection<Activity>(conn.Query<Activity>(
                    "SELECT * FROM Activity " +
                    "WHERE IsDone = 0 " +
                    "AND List = 'Zaplanowane' " +
                    "AND StartHour IS NOT NULL " +
                    "AND StartDate >= '" + TodayTemp.UtcTicks + "'" + // TODO: pamiętać o usunięciu znaku '>'
                    "ORDER BY PriorityId DESC, EstimationId ASC, StartHour ASC"
                    ).ToList());

            // w pierwszej iteracji każdą taką znalezioną aktywność spróbuj dodać do planu dnia pod odpowiednią godziną
            foreach (var act in tempActivityList)
            {
                int actId = act.StartHour.Value.Hours;
                int duration = GetActivityDuration(act);

                #region Refresh Day Plan Duration
                // jeśli aktywność zaczyna się później, niż koniec planu dnia - wydłuż plan dnia
                if (actId > EndHour)
                {
                    for (int i = EndHour + 1; i < actId + 1; i++)
                        PlannedDay1.Add(new PlannedActivity(i));
                    EndHour = actId + duration - 1;
                }

                // jeśli aktywność zaczyna się wcześniej, niż początek planu dnia - zacznij plan dnia wcześniej
                if (actId < StartHour)
                {
                    for (int i = StartHour - 1; i > actId - 1; i--)
                        PlannedDay1.Insert(0, new PlannedActivity(i));
                    StartHour = actId;
                }
                #endregion

                // jeśli aktywność zaczyna się w godzinach planu dnia - dodaj ją do planu dnia
                for (int i = 0; i < PlannedDay1.Count; i++)
                {
                    //Debug.WriteLine("EndHour: " + EndHour + " Przetwarzana aktywność (temp): " + act.Title + " PlannedDay1[i].Id: " + PlannedDay1[i].Id + " actId: " + actId);

                    if (PlannedDay1[i].Id == actId)
                    {
                        bool CanBeAdded = CanActivityBeAdded(i, duration);

                        // dodaj ją do planu dnia tylko, jeśli będzie mogła się znaleźć pod "swoją" godziną
                        if (PlannedDay1[i].ProposedActivity.IsPlaceholder == true && CanBeAdded == true)
                        {
                            PlannedDay1[i].ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(act.Id);
                            RemoveReservedItems(i, duration);
                            PlannedDay1[i].ItemHeight = 60 * duration;
                        }
                        // jeśli aktywność koliduje z inną (już dodaną) aktywnością, dodaj ją do listy nieobsłużonych aktywności
                        else
                            tempUnhandledActivityList.Add(act);
                        break;
                    }
                    // jeśli aktywność koliduje z inną (już dodaną) aktywnością, dodaj ją do listy nieobsłużonych aktywności
                    else if (PlannedDay1[i].Id > actId )
                    {
                        tempUnhandledActivityList.Add(act);
                        break;
                    }
                }

                if (PlannedDay1[PlannedDay1.Count - 1].Id < actId)
                    tempUnhandledActivityList.Add(act);
            }

            // w drugiej iteracji każdą nieobsłużoną aktywność spróbuj dodać do planu dnia
            foreach (var act in tempUnhandledActivityList)
            {
                int actId = act.StartHour.Value.Hours;
                int duration = GetActivityDuration(act);

                for (int i = 0; i < PlannedDay1.Count; i++)
                {
                    int tempId = PlannedDay1[i].Id;
                    if (i > 0 && PlannedDay1[i].Id < PlannedDay1[i - 1].Id)
                        tempId += 24;

                    if (tempId >= actId)
                    {
                        bool CanBeAdded = CanActivityBeAdded(i, duration);

                        // dodaj aktywność pod odpowiednią godziną, tylko jeśli jest miejsce
                        // jeśli nie ma miejsca, to spróbuj wstawić aktywność godzinę później
                        while (i < PlannedDay1.Count - 1 && (
                            (PlannedDay1[i].ProposedActivity.IsPlaceholder != true) ||
                            (CanBeAdded == false)))
                        {
                            i++;
                            CanBeAdded = CanActivityBeAdded(i, duration);
                        }

                        if (PlannedDay1[i].ProposedActivity.IsPlaceholder == true && CanBeAdded == true)
                        {
                            PlannedDay1[i].ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(act.Id);
                            RemoveReservedItems(i, duration);
                            PlannedDay1[i].ItemHeight = 60 * duration;
                            PlannedDay1[i].HourColor = "Red";
                        }
                        // jeśli aktywność zaczyna się później, niż koniec dnia - dodaj ją mimo wszystko
                        // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                        else
                        {
                            PlannedDay1.Add(new PlannedActivity((PlannedDay1[i].Id + GetPreviousActivityDuration(i)) % 24, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
                            PlannedDay1[PlannedDay1.Count - 1].ItemHeight = 60 * duration;
                            PlannedDay1[PlannedDay1.Count - 1].HourColor = "Red";
                        }
                        break;
                    }
                }
            }


            // --------------- KROKI 3, 4, 5, 6 ---------------
            // w kroku nr 3 trzeba dodać dzisiejsze, niewykonane aktywności, ale BEZ godziny
            // w kroku nr 4 trzeba dodać zaplanowane, ale niewykonane aktywności z poprzednich dni
            // w kroku nr 5 trzeba dodać niewykonane aktywności z listy "najbliższe"
            // w kroku nr 6 trzeba dodać niewykonane aktywności z listy "kiedyś"

            // znajdź w localdb odpowiednie aktywności dla kroków od 3 do 6
            // posortuj znalezione aktywności wg kroków, od tych z najwyższym priorytetem i od najkrótszej
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                tempActivityList = new ObservableCollection<Activity>(conn.Query<Activity>(
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND List = 'Zaplanowane' AND StartHour IS NULL AND StartDate >= '" + TodayTemp.UtcTicks +
                        // TODO: pamiętać o usunięciu znaku '>'
                        "'ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND List = 'Zaplanowane' AND StartDate < '" + TodayTemp.UtcTicks +
                        "'ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND List = 'Najblizsze' " +
                        "ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND List = 'Kiedys' " +
                        "ORDER BY PriorityId DESC, EstimationId ASC)"
                    ).ToList());

            // każdą taką znalezioną aktywność dodaj do planu dnia pod pierwszą wolną godziną
            // ale nie wstawiaj aktywności, jeśli wolne miejsce zawiera się w godzinach pracy
            foreach (var act in tempActivityList)
            {
                bool CanBeAdded = true;
                int duration = GetActivityDuration(act);

                // jeśli nie ma miejsca, to spróbuj wstawić aktywność godzinę później
                int i = 0;
                while (i < PlannedDay1.Count - 1 && (
                    (PlannedDay1[i].ProposedActivity.IsPlaceholder != true) ||
                    (PlannedDay1[i].Id >= StartWork && PlannedDay1[i].Id < EndWork) ||
                    (CanBeAdded == false)))
                {
                    i++;
                    CanBeAdded = CanActivityBeAdded(i, duration);

                    // sprawdź, czy poranna aktywność nie będzie kończyła się po rozpoczęciu pracy lub po końcu dnia
                    if ((PlannedDay1[i].Id < StartWork && StartWork < (PlannedDay1[i].Id + duration)) ||
                        (act.List != "Zaplanowane" && (EndHour + 1 < (PlannedDay1[i].Id + duration))))
                        CanBeAdded = false;
                }

                if (PlannedDay1[i].ProposedActivity.IsPlaceholder == true && CanBeAdded == true)
                {
                    PlannedDay1[i].ProposedActivity = LocalDatabaseHelper.ReadItem<Activity>(act.Id);
                    RemoveReservedItems(i, duration);
                    PlannedDay1[i].ItemHeight = 60 * duration;

                    // dodatkowo datę aktywności z poprzednich dni oznacz kolorem czerwonym
                    if (PlannedDay1[i].ProposedActivity.StartDate < TodayTemp)
                    {
                        PlannedDay1[i].DateColor = "Red";
                        PlannedDay1[i].HourColor = "Red";
                    }
                }

                // jeśli aktywność zostanie zaproponowana później, niż koniec dnia:
                // a) jeśli zaczyna się wcześniej niż dzisiaj - nie proponuj jej
                // b) jeśli zaczyna się dzisiaj - zaproponuj ją mimo wszystko
                // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                else if (act.StartDate >= TodayTemp) // TODO: pamiętać o usunięciu znaku '>'
                {
                    PlannedDay1.Add(new PlannedActivity((PlannedDay1[i].Id + GetPreviousActivityDuration(i)) % 24, LocalDatabaseHelper.ReadItem<Activity>(act.Id)));
                    PlannedDay1[i + 1].ItemHeight = 60 * duration;
                }
            }

            // KROK 7: Pomiń listę "Nowe" - takie aktywności należy najpierw przejrzeć i umieścić na jakiejś liście
            // KROK 8: Pomiń listę "Oddelegowane" - ktoś takie aktywności musi zrobić za nas
        }

        #region Filling Planned Day Helpers

        private bool CanActivityBeAdded(int i, int duration)
        {
            // sprawdź, czy wszystkie pola dla wstawianej aktywności w miejscu 'i' są puste
            bool canBeAdded = true;
            for (int j = i; j < i + duration; j++)
            {
                if (j < PlannedDay1.Count && PlannedDay1[j].ProposedActivity.IsPlaceholder != true)
                {
                    canBeAdded = false;
                    break;
                }
            }
            return canBeAdded;
        }

        private void RemoveReservedItems(int i, int duration)
        {
            // usuń tyle kolejnych itemów za dodaną właśnie aktywnością w miejscu 'i', ile wynosi jej estymacja
            for (int j = i + duration - 1; j > i; j--)
            {
                if (j < PlannedDay1.Count)
                {
                    PlannedDay1.RemoveAt(j);
                }
            }
        }

        private int GetActivityDuration(Activity act)
        {
            var tempEstimationList = LocalDatabaseHelper.ReadAllItemsFromTable<Estimation>();
            int duration = 0;

            if (act.EstimationId > 2)
                duration = (int)tempEstimationList[(int)act.EstimationId - 1].Duration;
            else
                duration = 1;

            return duration;
        }

        private int GetPreviousActivityDuration(int i)
        {
            var tempEstimationList = LocalDatabaseHelper.ReadAllItemsFromTable<Estimation>();
            int? prevActEstId = PlannedDay1[i].ProposedActivity.EstimationId;
            int prevActDuration = 0;

            if (prevActEstId > 2)
                prevActDuration = (int)tempEstimationList[(int)prevActEstId - 1].Duration;
            else
                prevActDuration = 1;

            return prevActDuration;
        }
        #endregion
    }
}
