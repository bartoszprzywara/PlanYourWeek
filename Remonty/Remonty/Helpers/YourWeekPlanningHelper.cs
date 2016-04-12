using Remonty.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Remonty.Helpers
{
    public class YourWeekPlanningHelper
    {
        public void GetPlannedWeek()
        {
            this.PlanYourWeek();
            Debug.WriteLine("Your Week has just been reloaded (" + DateTime.Now + ")");
            App.FinalPlannedWeek = PlannedWeek;
        }

        private void PlanYourWeek()
        {
            this.GetSettings();
            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 0");

            for (int i = 0; i < 7; i++)
            {
                PlannedWeek[i] = new ObservableCollection<PlannedActivity>();
                this.PlanYourDay(i);
            }
        }

        private int DayLimit = 26; // 02:00 AM
        private bool[] WorkingHoursEnabled = new bool[7];
        public int[] StartHour = new int[7];
        private int[] StartWork = new int[7];
        private int[] EndWork = new int[7];
        private int[] EndHour = new int[7];
        private ObservableCollection<PlannedActivity>[] PlannedWeek = new ObservableCollection<PlannedActivity>[7];

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

        private void PlanYourDay(int day)
        {
            // TODO: zrobić sobotę i niedzielę niepodatnymi na godziny pracy
            // TODO: przemyśleć sprawę połówek godziny i startów aktywności o nierównej godzinie


            // --------------- KROK 1 ---------------
            // w kroku nr 1 trzeba wypełnić plan dnia placeholderami

            // wypełnij plan dnia pustymi aktywnościami - placeholderami
            for (int i = StartHour[day]; i <= EndHour[day]; i++)
                PlannedWeek[day].Add(new PlannedActivity(i));


            // --------------- KROK 2 ---------------
            // w kroku nr 2 trzeba dodać dzisiejsze, niewykonane aktywności z ustaloną godziną

            // znajdź w localdb zaplanowane, ale niewykonane aktywności, które zaczynają się dzisiaj i mają określoną godzinę
            // posortuj znalezione aktywności od tych z najwyższym priorytetem i od najkrótszej
            // na koniec posortuj je godziną rozpoczęcia od najpóźniejszej
            ObservableCollection<Activity> tempActivityList;
            ObservableCollection<Activity> tempUnhandledActivityList = new ObservableCollection<Activity>();
            DateTimeOffset tempToday = ((DateTimeOffset.Now).Date + new TimeSpan(0, 0, 0)).AddDays(day);

            using (LocalDatabaseHelper.conn.Lock())
                tempActivityList = new ObservableCollection<Activity>(LocalDatabaseHelper.conn.Query<Activity>(
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
                        PlannedWeek[day].Add(new PlannedActivity(i));
                    EndHour[day] = actId + duration - 1;
                }

                // jeśli aktywność zaczyna się wcześniej, niż początek planu dnia - zacznij plan dnia wcześniej
                if (actId < StartHour[day])
                {
                    for (int i = StartHour[day] - 1; i > actId - 1; i--)
                        PlannedWeek[day].Insert(0, new PlannedActivity(i));
                    StartHour[day] = actId;
                }
                #endregion

                // jeśli aktywność zaczyna się w godzinach planu dnia - dodaj ją do planu dnia
                for (int i = 0; i < PlannedWeek[day].Count; i++)
                {
                    if (PlannedWeek[day][i].Id + GetPreviousActivityDuration(day, i) - 1 == actId)
                    {
                        IsActivityFound = true;
                        bool CanBeAdded = CanActivityBeAdded(day, i, duration);

                        // dodaj ją do planu dnia tylko, jeśli będzie mogła się znaleźć pod "swoją" godziną
                        if (PlannedWeek[day][i].ProposedActivity == null && CanBeAdded)
                        {
                            PlannedWeek[day][i].ProposedActivity = act;
                            RemoveReservedItems(day, i, duration);
                            PlannedWeek[day][i].ItemHeight = CalculateHeight(duration);
                            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                        }
                        // jeśli aktywność koliduje z inną (już dodaną) aktywnością, dodaj ją do listy nieobsłużonych aktywności
                        else
                            tempUnhandledActivityList.Add(act);
                        break;
                    }
                    // jeśli aktywność koliduje z inną (już dodaną) aktywnością, dodaj ją do listy nieobsłużonych aktywności
                    else if (PlannedWeek[day][i].Id + GetPreviousActivityDuration(day, i) - 1 > actId)
                    {
                        IsActivityFound = true;
                        tempUnhandledActivityList.Add(act);
                        break;
                    }
                }

                if (!IsActivityFound)
                {
                    // pierwotny if: (PlannedWeek[day][PlannedWeek[day].Count - 1].Id < actId)
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

                for (int i = 0; i < PlannedWeek[day].Count; i++)
                {
                    if (PlannedWeek[day][i].Id + GetPreviousActivityDuration(day, i) - 1 >= actId)
                    {
                        IsActivityFound = true;
                        bool CanBeAdded = CanActivityBeAdded(day, i, duration);

                        // dodaj aktywność pod odpowiednią godziną, tylko jeśli jest miejsce
                        // jeśli nie ma miejsca, to spróbuj wstawić aktywność godzinę później
                        while (i < PlannedWeek[day].Count - 1 && (
                            (PlannedWeek[day][i].ProposedActivity != null) ||
                            (!CanBeAdded)))
                        {
                            i++;
                            CanBeAdded = CanActivityBeAdded(day, i, duration);
                        }

                        if (PlannedWeek[day][i].ProposedActivity == null && CanBeAdded)
                        {
                            PlannedWeek[day][i].ProposedActivity = act;
                            RemoveReservedItems(day, i, duration);
                            PlannedWeek[day][i].ItemHeight = CalculateHeight(duration);
                            PlannedWeek[day][i].HourColor = "Red";
                            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                        }
                        // jeśli aktywność zaczyna się później, niż koniec dnia - dodaj ją mimo wszystko
                        // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                        // nie dodawaj aktywnośći w ogóle, jeśli przekroczy tzw. "limit dnia"
                        else if (GetTemporaryPlannedActivityId(day, i) + duration - 1 < DayLimit)
                        {
                            PlannedWeek[day].Add(new PlannedActivity((PlannedWeek[day][i].Id + GetPreviousActivityDuration(day, i)) % 24, act));
                            PlannedWeek[day][PlannedWeek[day].Count - 1].ItemHeight = CalculateHeight(duration);
                            PlannedWeek[day][PlannedWeek[day].Count - 1].HourColor = "Red";
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
            using (LocalDatabaseHelper.conn.Lock())
                tempActivityList = new ObservableCollection<Activity>(LocalDatabaseHelper.conn.Query<Activity>(
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
                while (i < PlannedWeek[day].Count - 1 && (
                    // sprawdź, czy pierwsze godzina dla wstawianej aktywności jest pusta w planie dnia
                    (PlannedWeek[day][i].ProposedActivity != null) ||
                    // sprawdź, czy aktywność nie będzie zaczynać sie w godzinach pracy
                    (PlannedWeek[day][i].Id >= StartWork[day] && PlannedWeek[day][i].Id < EndWork[day]) ||
                    // sprawdź, czy aktywność nie będzie kończyła się po rozpoczęciu pracy
                    (PlannedWeek[day][i].Id < StartWork[day] && StartWork[day] < (PlannedWeek[day][i].Id + duration)) ||
                    (!CanBeAdded)))
                {
                    i++;
                    CanBeAdded = CanActivityBeAdded(day, i, duration);

                    // sprawdź, czy niedzisiejsza aktywność nie będzie kończyła się po końcu dnia
                    if (act.StartDate != tempToday && (EndHour[day] + 1 < (PlannedWeek[day][i].Id + duration)))
                        CanBeAdded = false;
                }

                if (PlannedWeek[day][i].ProposedActivity == null && CanBeAdded)
                {
                    PlannedWeek[day][i].ProposedActivity = act;
                    RemoveReservedItems(day, i, duration);
                    PlannedWeek[day][i].ItemHeight = CalculateHeight(duration);

                    // dodatkowo datę aktywności z poprzednich dni oznacz kolorem czerwonym
                    if (PlannedWeek[day][i].ProposedActivity.StartDate < tempToday)
                    {
                        PlannedWeek[day][i].DateColor = "Red";
                        PlannedWeek[day][i].HourColor = "Red";
                    }

                    LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                }

                // jeśli aktywność zostanie zaproponowana później, niż koniec dnia:
                // a) jeśli zaczyna się wcześniej niż dzisiaj - nie proponuj jej
                // b) jeśli zaczyna się dzisiaj - zaproponuj ją mimo wszystko
                // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                else if (act.StartDate == tempToday && GetTemporaryPlannedActivityId(day, i) + duration - 1 < DayLimit)
                {
                    PlannedWeek[day].Add(new PlannedActivity((PlannedWeek[day][i].Id + GetPreviousActivityDuration(day, i)) % 24, act));
                    PlannedWeek[day][i + 1].ItemHeight = CalculateHeight(duration);
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
                if (j < PlannedWeek[day].Count && PlannedWeek[day][j].ProposedActivity != null)
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
                if (j < PlannedWeek[day].Count)
                {
                    PlannedWeek[day].RemoveAt(j);
                }
            }
        }

        public int GetActivityDuration(Activity act)
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
            int? prevActEstId = PlannedWeek[day][i].ProposedActivity?.EstimationId;
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
            if (i == 0) return PlannedWeek[day][i].Id;

            if (PlannedWeek[day][i].Id < PlannedWeek[day][i - 1].Id)
                IdDecreased = true;

            if (IdDecreased)
                return PlannedWeek[day][i].Id + 24;
            else
                return PlannedWeek[day][i].Id;
        }

        private int CalculateHeight(int duration)
        {
            //return 60 * duration;
            return 55 + 40 * (duration - 1);
        }

        public class CompletelyUnhandledActivityException : Exception
        {
            public CompletelyUnhandledActivityException(string message) : base(message) { }
        }

        #endregion
    }
}
