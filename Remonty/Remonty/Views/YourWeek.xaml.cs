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
            GetPlannedWeek();
            YourWeekPivot.SelectedIndex = App.LastPivotItem;
        }

        private void GetSettings()
        {
            ObservableCollection<Settings> savedSettings = LocalDatabaseHelper.ReadAllItemsFromTable<Settings>();

            StartHour[0] = TimeSpan.Parse(savedSettings[0].Value).Hours;
            StartWork[0] = TimeSpan.Parse(savedSettings[1].Value).Hours;
            EndWork[0] = TimeSpan.Parse(savedSettings[2].Value).Hours;
            EndHour[0] = TimeSpan.Parse(savedSettings[3].Value).Hours - 1;
            if (EndHour[0] < 0 || EndHour[0] < StartHour[0])
                EndHour[0] += 24;
            WorkingHoursEnabled[0] = bool.Parse(savedSettings[4].Value);

            if (!WorkingHoursEnabled[0])
            {
                StartWork[0] = -1;
                EndWork[0] = -1;
            }

            for (int i = 1; i < 7; i++)
            {
                StartHour[i] = StartHour[0];
                StartWork[i] = StartWork[0];
                EndWork[i] = EndWork[0];
                EndHour[i] = EndHour[0];
                WorkingHoursEnabled[i] = WorkingHoursEnabled[0];
            }
        }

        public void GetPlannedWeek()
        {
            if (App.PlanNeedsToBeReloaded)
            {
                PlanYourWeek();
                Debug.WriteLine("Your Week has just been reloaded (" + DateTime.Now + ")");
                App.BackupWeekPlan = PlannedDay;
            }
            else
                PlannedDay = App.BackupWeekPlan;

            App.PlanNeedsToBeReloaded = false;
        }

        private void PlanYourWeek()
        {
            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 0");
            App.PlanNeedsToBeReloaded = true;

            for (int i = 0; i < 7; i++)
            {
                PlannedDay[i] = new ObservableCollection<PlannedActivity>();
                PlanYourDay(i);
            }
        }

        private int currentPivotItem = 0;
        private int DayLimit = 26; // 02:00
        private bool[] WorkingHoursEnabled = new bool[7];
        private int[] StartHour = new int[7];
        private int[] StartWork = new int[7];
        private int[] EndWork = new int[7];
        private int[] EndHour = new int[7];
        private ObservableCollection<PlannedActivity>[] PlannedDay = new ObservableCollection<PlannedActivity>[7];

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

            int duration = 0;
            int i = 0;
            while (PlannedDay[currentPivotItem][i].ProposedActivity?.Id != ItemId)
            {
                if (PlannedDay[currentPivotItem][i].ProposedActivity != null)
                    duration += (GetActivityDuration(PlannedDay[currentPivotItem][i].ProposedActivity) - 1);
                i++;
            }
            for (int j = i + 1; j < i + GetActivityDuration(PlannedDay[currentPivotItem][i].ProposedActivity); j++)
                PlannedDay[currentPivotItem].Insert(j, new PlannedActivity(j + StartHour[currentPivotItem] + duration));
            PlannedDay[currentPivotItem][i] = new PlannedActivity(i + StartHour[currentPivotItem] + duration);
        }

        private void YourWeekPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentPivotItem = YourWeekPivot.SelectedIndex;
            App.LastPivotItem = currentPivotItem;
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

        private void PlanYourDay(int day)
        {
            // TODO: zrobić sobotę i niedzielę niepodatnymi na godziny pracy
            // TODO: przemyśleć sprawę połówek godziny i startów aktywności o nierównej godzinie


            // --------------- KROK 1 ---------------
            // w kroku nr 1 trzeba wypełnić plan dnia placeholderami

            // wypełnij plan dnia pustymi aktywnościami - placeholderami
            for (int i = StartHour[day]; i <= EndHour[day]; i++)
                PlannedDay[day].Add(new PlannedActivity(i));


            // --------------- KROK 2 ---------------
            // w kroku nr 2 trzeba dodać dzisiejsze, niewykonane aktywności z ustaloną godziną

            // znajdź w localdb zaplanowane, ale niewykonane aktywności, które zaczynają się dzisiaj i mają określoną godzinę
            // posortuj znalezione aktywności od tych z najwyższym priorytetem i od najkrótszej
            // na koniec posortuj je godziną rozpoczęcia od najpóźniejszej
            ObservableCollection<Activity> tempActivityList;
            ObservableCollection<Activity> tempUnhandledActivityList = new ObservableCollection<Activity>();
            DateTimeOffset tempToday = ((DateTimeOffset.Now).Date + new TimeSpan(0, 0, 0)).AddDays(day);

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), LocalDatabaseHelper.sqlpath))
                tempActivityList = new ObservableCollection<Activity>(conn.Query<Activity>(
                    "SELECT * FROM Activity " +
                    "WHERE IsDone = 0 " +
                    "AND IsAdded = 0 " +
                    "AND List = 'Zaplanowane' " +
                    "AND StartHour IS NOT NULL " +
                    "AND StartDate = '" + tempToday.UtcTicks + "'" +
                    "ORDER BY PriorityId DESC, EstimationId ASC, StartHour ASC"
                    ).ToList());

            // w pierwszej iteracji każdą taką znalezioną aktywność spróbuj dodać do planu dnia pod odpowiednią godziną
            foreach (var act in tempActivityList)
            {
                int actId = act.StartHour.Value.Hours;
                int duration = GetActivityDuration(act);
                bool IsActivityFound = false;

                #region Refresh Day Plan Duration
                // jeśli aktywność zaczyna się później, niż koniec planu dnia - wydłuż plan dnia
                if (actId > EndHour[day])
                {
                    for (int i = EndHour[day] + 1; i < actId + 1; i++)
                        PlannedDay[day].Add(new PlannedActivity(i));
                    EndHour[day] = actId + duration - 1;
                }

                // jeśli aktywność zaczyna się wcześniej, niż początek planu dnia - zacznij plan dnia wcześniej
                if (actId < StartHour[day])
                {
                    for (int i = StartHour[day] - 1; i > actId - 1; i--)
                        PlannedDay[day].Insert(0, new PlannedActivity(i));
                    StartHour[day] = actId;
                }
                #endregion

                // jeśli aktywność zaczyna się w godzinach planu dnia - dodaj ją do planu dnia
                for (int i = 0; i < PlannedDay[day].Count; i++)
                {
                    if (PlannedDay[day][i].Id + GetPreviousActivityDuration(day, i) - 1 == actId)
                    {
                        IsActivityFound = true;
                        bool CanBeAdded = CanActivityBeAdded(day, i, duration);

                        // dodaj ją do planu dnia tylko, jeśli będzie mogła się znaleźć pod "swoją" godziną
                        if (PlannedDay[day][i].ProposedActivity == null && CanBeAdded)
                        {
                            PlannedDay[day][i].ProposedActivity = act;
                            RemoveReservedItems(day, i, duration);
                            PlannedDay[day][i].ItemHeight = 60 * duration;
                            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                        }
                        // jeśli aktywność koliduje z inną (już dodaną) aktywnością, dodaj ją do listy nieobsłużonych aktywności
                        else
                            tempUnhandledActivityList.Add(act);
                        break;
                    }
                    // jeśli aktywność koliduje z inną (już dodaną) aktywnością, dodaj ją do listy nieobsłużonych aktywności
                    else if (PlannedDay[day][i].Id + GetPreviousActivityDuration(day, i) - 1 > actId)
                    {
                        IsActivityFound = true;
                        tempUnhandledActivityList.Add(act);
                        break;
                    }
                }

                if (!IsActivityFound)
                {
                    // pierwotny if: (PlannedDay[day][PlannedDay[day].Count - 1].Id < actId)
                    // dla pewności zostawię tego if-a, gdyby kiedyś jakaś aktywność tutaj trafiła
                    throw new CompletelyUnhandledActivityException("Nieobsłużona aktywność: " + act.Title);
                }
            }

            // w drugiej iteracji każdą nieobsłużoną aktywność spróbuj dodać do planu dnia
            foreach (var act in tempUnhandledActivityList)
            {
                int actId = act.StartHour.Value.Hours;
                int duration = GetActivityDuration(act);
                bool IsActivityFound = false;

                for (int i = 0; i < PlannedDay[day].Count; i++)
                {
                    if (PlannedDay[day][i].Id + GetPreviousActivityDuration(day, i) - 1 >= actId)
                    {
                        IsActivityFound = true;
                        bool CanBeAdded = CanActivityBeAdded(day, i, duration);

                        // dodaj aktywność pod odpowiednią godziną, tylko jeśli jest miejsce
                        // jeśli nie ma miejsca, to spróbuj wstawić aktywność godzinę później
                        while (i < PlannedDay[day].Count - 1 && (
                            (PlannedDay[day][i].ProposedActivity != null) ||
                            (!CanBeAdded)))
                        {
                            i++;
                            CanBeAdded = CanActivityBeAdded(day, i, duration);
                        }

                        if (PlannedDay[day][i].ProposedActivity == null && CanBeAdded)
                        {
                            PlannedDay[day][i].ProposedActivity = act;
                            RemoveReservedItems(day, i, duration);
                            PlannedDay[day][i].ItemHeight = 60 * duration;
                            PlannedDay[day][i].HourColor = "Red";
                            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                        }
                        // jeśli aktywność zaczyna się później, niż koniec dnia - dodaj ją mimo wszystko
                        // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                        // nie dodawaj aktywnośći w ogóle, jeśli przekroczy limit dnia
                        else if (GetTemporaryPlannedActivityId(day, i) + duration - 1 < DayLimit)
                        {
                            PlannedDay[day].Add(new PlannedActivity((PlannedDay[day][i].Id + GetPreviousActivityDuration(day, i)) % 24, act));
                            PlannedDay[day][PlannedDay[day].Count - 1].ItemHeight = 60 * duration;
                            PlannedDay[day][PlannedDay[day].Count - 1].HourColor = "Red";
                            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                        }
                        break;
                    }
                }
                if (!IsActivityFound)
                {
                    // dla pewności zostawię tego if-a, gdyby kiedyś jakaś aktywność tutaj trafiła
                    throw new CompletelyUnhandledActivityException("Nieobsłużona aktywność: " + act.Title);
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
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND IsAdded = 0 AND List = 'Zaplanowane' AND StartHour IS NULL AND StartDate = '" + tempToday.UtcTicks +
                        "'ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND IsAdded = 0 AND List = 'Zaplanowane' AND StartDate < '" + tempToday.AddDays(-day).UtcTicks +
                        "'ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND IsAdded = 0 AND List = 'Najblizsze' " +
                        "ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND IsAdded = 0 AND List = 'Kiedys' " +
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
                while (i < PlannedDay[day].Count - 1 && (
                    (PlannedDay[day][i].ProposedActivity != null) ||
                    (PlannedDay[day][i].Id >= StartWork[day] && PlannedDay[day][i].Id < EndWork[day]) ||
                    (!CanBeAdded)))
                {
                    i++;
                    CanBeAdded = CanActivityBeAdded(day, i, duration);

                    // sprawdź, czy poranna aktywność nie będzie kończyła się po rozpoczęciu pracy lub po końcu dnia
                    if ((PlannedDay[day][i].Id < StartWork[day] && StartWork[day] < (PlannedDay[day][i].Id + duration)) ||
                        (act.List != "Zaplanowane" && (EndHour[day] + 1 < (PlannedDay[day][i].Id + duration))))
                        CanBeAdded = false;
                }

                if (PlannedDay[day][i].ProposedActivity == null && CanBeAdded)
                {
                    PlannedDay[day][i].ProposedActivity = act;
                    RemoveReservedItems(day, i, duration);
                    PlannedDay[day][i].ItemHeight = 60 * duration;

                    // dodatkowo datę aktywności z poprzednich dni oznacz kolorem czerwonym
                    if (PlannedDay[day][i].ProposedActivity.StartDate < tempToday)
                    {
                        PlannedDay[day][i].DateColor = "Red";
                        PlannedDay[day][i].HourColor = "Red";
                    }

                    LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                }

                // jeśli aktywność zostanie zaproponowana później, niż koniec dnia:
                // a) jeśli zaczyna się wcześniej niż dzisiaj - nie proponuj jej
                // b) jeśli zaczyna się dzisiaj - zaproponuj ją mimo wszystko
                // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                else if (act.StartDate == tempToday && GetTemporaryPlannedActivityId(day, i) + duration - 1 < DayLimit)
                {
                    PlannedDay[day].Add(new PlannedActivity((PlannedDay[day][i].Id + GetPreviousActivityDuration(day, i)) % 24, act));
                    PlannedDay[day][i + 1].ItemHeight = 60 * duration;
                    LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                }
            }

            // KROK 7: Pomiń listę "Nowe" - takie aktywności musimy najpierw przejrzeć i umieścić na jakiejś liście
            // KROK 8: Pomiń listę "Oddelegowane" - takie aktywności ktoś musi zrobić za nas
        }

        #region Filling Planned Day Helpers

        private bool CanActivityBeAdded(int day, int i, int duration)
        {
            // sprawdź, czy wszystkie pola dla wstawianej aktywności w miejscu 'i' są puste
            bool canBeAdded = true;
            for (int j = i; j < i + duration; j++)
            {
                if (j < PlannedDay[day].Count && PlannedDay[day][j].ProposedActivity != null)
                {
                    canBeAdded = false;
                    break;
                }
            }
            return canBeAdded;
        }

        private void RemoveReservedItems(int day, int i, int duration)
        {
            // usuń tyle kolejnych itemów za dodaną właśnie aktywnością w miejscu 'i', ile wynosi jej estymacja
            for (int j = i + duration - 1; j > i; j--)
            {
                if (j < PlannedDay[day].Count)
                {
                    PlannedDay[day].RemoveAt(j);
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

        private int GetPreviousActivityDuration(int day, int i)
        {
            var tempEstimationList = LocalDatabaseHelper.ReadAllItemsFromTable<Estimation>();
            int? prevActEstId = PlannedDay[day][i].ProposedActivity?.EstimationId;
            int prevActDuration = 0;

            if (prevActEstId > 2)
                prevActDuration = (int)tempEstimationList[(int)prevActEstId - 1].Duration;
            else
                prevActDuration = 1;

            return prevActDuration;
        }

        private bool IdDecreased = false;

        private int GetTemporaryPlannedActivityId(int day, int i)
        {
            if (i == 0) return PlannedDay[day][i].Id;

            if (PlannedDay[day][i].Id < PlannedDay[day][i - 1].Id)
                IdDecreased = true;

            if (IdDecreased)
                return PlannedDay[day][i].Id + 24;
            else
                return PlannedDay[day][i].Id;
        }

        public class CompletelyUnhandledActivityException : Exception
        {
            public CompletelyUnhandledActivityException(string message) : base(message) { }
        }

        #endregion
    }
}
