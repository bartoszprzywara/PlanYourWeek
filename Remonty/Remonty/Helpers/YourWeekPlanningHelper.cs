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
            // TODO: przemyśleć sprawę połówek godziny i startów aktywności o nierównej godzinie
            // TODO: zadania z częściami godziny - może po prostu wstawiać (insert item) aktywność między inne na plan?
            // TODO: zrobić przypomnienia dla zaplanowanych zadań

            this.PlanYourWeek();
            Debug.WriteLine("Your Week has just been reloaded (" + DateTime.Now + ")");
            App.FinalPlannedWeekItems.PlannedWeek = PlannedWeek;
            App.FinalPlannedWeekItems.TotalHours = TotalHours;
            App.FinalPlannedWeekItems.UsedHours = UsedHours;
            App.FinalPlannedWeekItems.TotalWorkingHours = TotalWorkingHours;
            App.FinalPlannedWeekItems.UsedWorkingHours = UsedWorkingHours;
        }

        private void PlanYourWeek()
        {
            // --------------- KROK GŁÓWNY A ---------------
            // ustal godziny rozpoczęcia i zakończenia dnia oraz pracy
            // np. wstaję o 7, idę do pracy na 9, wracam o 17, idę spać o 23
            // dodatkowo ustal, czy godziny pracy mają być w ogóle brane pod uwagę
            this.GetSettings();
            // co daje krok A: wiemy, jaki zakres będzie miał plan dnia

            // --------------- KROK GŁÓWNY B ---------------
            // wszystkim aktywnościom w localdb wyzeruj flagę o dodaniu do planu tygodnia
            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 0");
            // co daje krok B: zgromadzone aktywności są przygotowane na dodawanie do planu dnia

            // --------------- KROK GŁÓWNY C ---------------
            // zaplanuj siedmiodniowy tydzień dzień po dniu
            // ponieważ to, co znajdzie się w aktualnie przetwarzanym dniu, zależy od zaplanowania poprzednich dni
            for (int i = 0; i < 7; i++)
            {
                PlannedWeek[i] = new ObservableCollection<PlannedActivity>();
                this.PlanYourDay(i);
            }
            // co daje krok C: mamy zaplanowany cały tydzień
        }

        private int DayLimit = 26; // 02:00 AM
        private bool[] DayweekWorkingHoursEnabled = new bool[7];
        private int[] StartHour = new int[7];
        private int[] StartWork = new int[7];
        private int[] EndWork = new int[7];
        private int[] EndHour = new int[7];
        private int[] TotalHours = new int[7];
        private int[] UsedHours = new int[7];
        private int[] TotalWorkingHours = new int[7];
        private int[] UsedWorkingHours = new int[7];
        private ObservableCollection<PlannedActivity>[] PlannedWeek = new ObservableCollection<PlannedActivity>[7];

        private void GetSettings()
        {
            // KROK GŁÓWNY A - szczegóły

            // z localdb pobierz aktualne ustawienia użytkownika i przypisz je wszystkim dniom tygodnia
            ObservableCollection<Settings> savedSettings = LocalDatabaseHelper.ReadAllItemsFromTable<Settings>();

            StartHour[0] = TimeSpan.Parse(savedSettings[0].Value).Hours;
            StartWork[0] = TimeSpan.Parse(savedSettings[1].Value).Hours;
            EndWork[0] = TimeSpan.Parse(savedSettings[2].Value).Hours;
            EndHour[0] = TimeSpan.Parse(savedSettings[3].Value).Hours;
            for (int i = 0; i < 7; i++)
                DayweekWorkingHoursEnabled[i] = bool.Parse(savedSettings[i + 4].Value);

            EndHour[0] = (EndHour[0] < StartHour[0]) ? EndHour[0] += 24 : EndHour[0];
            TotalHours[0] = (StartWork[0] - StartHour[0]) + (EndHour[0] - EndWork[0]);
            TotalWorkingHours[0] = EndWork[0] - StartWork[0];

            for (int i = 1; i < 7; i++)
            {
                StartHour[i] = StartHour[0];
                StartWork[i] = StartWork[0];
                EndWork[i] = EndWork[0];
                EndHour[i] = EndHour[0];
                TotalHours[i] = TotalHours[0];
                TotalWorkingHours[i] = TotalWorkingHours[0];
            }

            int j = 0;
            for (int i = (int)DateTime.Today.DayOfWeek; i < 7; i++, j++)
                if (!DayweekWorkingHoursEnabled[i])
                {
                    StartWork[j] = -1;
                    EndWork[j] = -1;
                    TotalHours[j] = EndHour[j] - StartHour[j];
                    TotalWorkingHours[j] = 0;
                }

            for (int i = 0; i < (int)DateTime.Today.DayOfWeek; i++, j++)
                if (!DayweekWorkingHoursEnabled[i])
                {
                    StartWork[j] = -1;
                    EndWork[j] = -1;
                    TotalHours[j] = EndHour[j] - StartHour[j];
                    TotalWorkingHours[j] = 0;
                }
        }

        private void PlanYourDay(int day)
        {
            // KROK GŁÓWNY C - szczegóły


            // --------------- KROK 1 ---------------
            // w kroku nr 1 trzeba wypełnić plan dnia placeholderami

            // wypełnij plan dnia pustymi aktywnościami - placeholderami
            for (int i = StartHour[day]; i < EndHour[day]; i++)
                PlannedWeek[day].Add(new PlannedActivity(i));
            // co daje krok 1: mamy przygotowany aktualnie przetwarzany dzień do wypełnienia go aktywnościami


            // --------------- KROK 2 ---------------
            // w kroku nr 2 trzeba dodać dzisiejsze, niewykonane aktywności z ustaloną godziną

            // krok 2a
            // znajdź w localdb zaplanowane, ale niewykonane aktywności, które zaczynają się w aktualnie przetwarzanym dniu i mają określoną godzinę
            // znajdź tylko takie aktywności, które nie zostały już wcześniej dodane do planu tygodnia w jednym z wcześniejszych dni
            // posortuj znalezione aktywności od tych z najwyższym priorytetem i następnie od najkrótszych
            // na koniec posortuj je ostatnim kryterium sortowania od najwcześniejszej godziny rozpoczęcia
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
            // co daje krok 2a: mamy listę aktywności zaplanowanych na aktualnie przetwarzany dzień z godzinami

            // krok 2b
            // w pierwszej iteracji każdą taką znalezioną aktywność spróbuj dodać do planu dnia pod odpowiednią, "swoją" godziną 
            foreach (var act in tempActivityList)
            {
                int actId = act.StartHour.Value.Hours;
                int duration = GetActivityDuration(act);
                bool IsActivityFound = false;

                #region Refresh Day Plan Duration
                // jeśli aktywność zaczyna się wcześniej, niż początek planu dnia - zacznij plan dnia wcześniej
                if (actId < StartHour[day])
                {
                    for (int i = StartHour[day] - 1; i > actId - 1; i--)
                        PlannedWeek[day].Insert(0, new PlannedActivity(i));
                    StartHour[day] = actId;
                }

                // jeśli aktywność zaczyna się później, niż koniec planu dnia - wydłuż plan dnia
                if (actId >= EndHour[day])
                {
                    for (int i = EndHour[day]; i < actId + 1; i++)
                        PlannedWeek[day].Add(new PlannedActivity(i));
                    EndHour[day] = actId + duration - 1;
                }
                #endregion

                // po ewentualnym wydłużeniu planu dnia - spróbuj dodać aktywność do planu dnia
                // przeszukaj aktualny plan dnia
                for (int i = 0; i < PlannedWeek[day].Count; i++)
                {
                    // podczas przeszukiwania, znajdź odpowiednie miejsce dla aktywności
                    if (PlannedWeek[day][i].Id + GetPreviousActivityDuration(day, i) - 1 == actId)
                    {
                        IsActivityFound = true;

                        // dodaj aktywność do planu dnia tylko wtedy, jeśli będzie mogła się znaleźć pod "swoją" godziną
                        // czyli "jej" godzina będzie niezajęta przez inną (wcześniej dodaną) aktywność
                        if (PlannedWeek[day][i].ProposedActivity == null && AreAllRequiredPlacesEmpty(day, i, duration))
                        {
                            PlannedWeek[day][i].ProposedActivity = act;
                            RemoveReservedItems(day, i, duration);
                            PlannedWeek[day][i].ItemHeight = CalculateHeight(duration);
                            // po dodaniu aktywności, oznacz ją w localdb jako dodaną
                            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                            UsedHours[day] += duration;
                        }
                        // jeśli aktywność jednak koliduje z inną (wcześniej dodaną) aktywnością,
                        // dodaj ją wtedy do listy tymczasowo nieobsłużonych aktywności
                        else
                            tempUnhandledActivityList.Add(act);
                        break;
                    }
                    // jeśli przeszukano cały aktualny plan dnia i nie znaleziono wolnego miejsca dla aktywności,
                    // dodaj ją wtedy do listy tymczasowo nieobsłużonych aktywności
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
            // co daje krok 2b: aktywności dla danego dnia z godzinami są umieszczone na planie tego dnia w "swoich" godzinach

            // krok 2c
            // w drugiej iteracji każdą tymczasowo nieobsłużoną aktywność spróbuj dodać do planu dnia
            // wiadomo, że taka aktywność nie będzie mogła się znaleźć pod "swoją" godziną
            foreach (var act in tempUnhandledActivityList)
            {
                int actId = act.StartHour.Value.Hours;
                int duration = GetActivityDuration(act);
                bool IsActivityFound = false;

                // przeszukaj aktualny plan dnia
                for (int i = 0; i < PlannedWeek[day].Count; i++)
                {
                    // podczas przeszukiwania, znajdź odpowiednie miejsce dla aktywności
                    // czyli takie, które jest w tym samym czasie, co aktywność, lub później
                    // ponieważ aktywność chcemy wstawić na plan nie wcześniej, niż się zaczyna
                    if (PlannedWeek[day][i].Id + GetPreviousActivityDuration(day, i) - 1 >= actId)
                    {
                        IsActivityFound = true;
                        bool CanBeAdded = AreAllRequiredPlacesEmpty(day, i, duration);

                        // dodaj aktywność pod znalezioną godziną, tylko jeśli jest miejsce
                        // jeśli nie ma miejsca, to spróbuj wstawić aktywność godzinę później
                        // powtarzaj czynność aż do skutku (czyli aż do końca planu dnia)
                        while (i < PlannedWeek[day].Count - 1 && (
                            (PlannedWeek[day][i].ProposedActivity != null) ||
                            (!CanBeAdded)))
                        {
                            i++;
                            CanBeAdded = AreAllRequiredPlacesEmpty(day, i, duration);
                        }

                        // jeśli znaleziono wolne miejsce w planie dnia, wstaw aktywność
                        if (PlannedWeek[day][i].ProposedActivity == null && CanBeAdded)
                        {
                            PlannedWeek[day][i].ProposedActivity = act;
                            RemoveReservedItems(day, i, duration);
                            PlannedWeek[day][i].ItemHeight = CalculateHeight(duration);
                            PlannedWeek[day][i].HourColor = "Red";
                            // po dodaniu aktywności, oznacz ją w localdb jako dodaną
                            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                            UsedHours[day] += duration;
                        }

                        // jeśli aktywność zaczyna się później, niż koniec dnia - dodaj ją mimo wszystko
                        // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                        // ale nie dodawaj aktywności w ogóle, jeśli przekroczy tzw. "limit dnia" - np. godzinę 3:00 w nocy
                        else if (GetTemporaryPlannedActivityId(day, i) + duration - 1 < DayLimit)
                        {
                            PlannedWeek[day].Add(new PlannedActivity((PlannedWeek[day][i].Id + GetPreviousActivityDuration(day, i)) % 24, act));
                            PlannedWeek[day][PlannedWeek[day].Count - 1].ItemHeight = CalculateHeight(duration);
                            PlannedWeek[day][PlannedWeek[day].Count - 1].HourColor = "Red";
                            // po dodaniu aktywności, oznacz ją w localdb jako dodaną
                            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                            UsedHours[day] += duration;
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
            // co daje krok 2c: tymczasowo nieobsłużone aktywności dla danego dnia z godzinami również są umieszczone na planie dnia

            // co daje krok 2: wszystkie aktywności zaplanowane na aktualnie przetwarzany dzień z godzinami są umieszczone na planie dnia (jeśli nie przekroczyły limitu dnia)


            // --------------- KROKI 3, 4, 5, 6 ---------------
            // w kroku nr 3 trzeba dodać zaplanowane na aktualnie przetwarzany dzień, niewykonane aktywności, ale BEZ ustalonej godziny
            // w kroku nr 4 trzeba dodać zaplanowane, ale niewykonane aktywności z poprzednich dni
            // w kroku nr 5 trzeba dodać niewykonane aktywności z listy "najbliższe"
            // w kroku nr 6 trzeba dodać niewykonane aktywności z listy "kiedyś"

            // krok 3a, 4a, 5a, 6a
            // za jednym razem znajdź w localdb odpowiednie aktywności dla kroków od 3 do 6
            // znajdź tylko takie aktywności, które nie zostały już wcześniej dodane do planu tygodnia w jednym z wcześniejszych dni
            // posortuj znalezione aktywności wg kroków, od tych z najwyższym priorytetem i następnie od najkrótszych
            // na koniec posortuj je ostatnim kryterium sortowania od najwcześniejszej godziny rozpoczęcia
            using (LocalDatabaseHelper.conn.Lock())
                tempActivityList = new ObservableCollection<Activity>(LocalDatabaseHelper.conn.Query<Activity>(
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND IsAdded = 0 AND List like 'Zaplanowane' AND StartHour IS NULL AND StartDate = '" + tempToday.UtcTicks +
                        "'ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND IsAdded = 0 AND List like 'Zaplanowane' AND StartDate < '" + tempToday.AddDays(-day).UtcTicks +
                        "'ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND IsAdded = 0 AND List like 'Najbli_sze' " +
                        "ORDER BY PriorityId DESC, EstimationId ASC) UNION ALL " +
                    "SELECT * FROM(SELECT * FROM Activity WHERE IsDone = 0 AND IsAdded = 0 AND List like 'Kiedy_' " +
                        "ORDER BY PriorityId DESC, EstimationId ASC)"
                    ).ToList());
            // co daje krok 3a, 4a, 5a, 6a: mamy listy odpowiednich aktywności dla kroków od 3 do 6

            // krok 3b, 4b, 5b, 6b
            // każdą taką znalezioną aktywność spróbuj dodać do planu dnia pod pierwszą wolną godziną
            // przetwarzane będą kolejno aktywności: zaplanowane na aktualnie przetwarzany dzień, aktywności zaplanowane na poprzednie dni,
            // następnie aktywności z listy "najbliższe" i na końcu aktywności z listy "kiedyś"
            foreach (var act in tempActivityList)
            {
                bool CanBeAdded = true;
                int duration = GetActivityDuration(act);

                // dodaj aktywność pod znalezioną godziną, tylko jeśli jest miejsce
                // jeśli nie ma miejsca, to spróbuj wstawić aktywność godzinę później
                // powtarzaj czynność aż do skutku (czyli aż do końca planu dnia)
                int i = 0;
                CanBeAdded = AreAllRequiredPlacesEmpty(day, i, duration);
                if (CanBeAdded)
                    CanBeAdded = AreAllRequiredHoursProper(day, i, duration);

                while (i < PlannedWeek[day].Count - 1 && !CanBeAdded)
                {
                    i++;
                    CanBeAdded = AreAllRequiredPlacesEmpty(day, i, duration);
                    if (CanBeAdded)
                        CanBeAdded = AreAllRequiredHoursProper(day, i, duration);

                    // jeśli aktywność nie jest zaplanowana na aktualnie przetwarzany dzień, to sprawdź, czy nie będzie kończyła się po końcu dnia
                    if (act.StartDate != tempToday && EndHour[day] < PlannedWeek[day][i].Id + duration)
                        CanBeAdded = false;
                }
                CanBeAdded = AreAllRequiredHoursProper(day, i, duration);
                if (act.StartDate != tempToday && EndHour[day] < PlannedWeek[day][i].Id + duration)
                    CanBeAdded = false;

                // jeśli znaleziono wolne miejsce w planie dnia, wstaw aktywność
                if (PlannedWeek[day][i].ProposedActivity == null && CanBeAdded)
                {
                    PlannedWeek[day][i].ProposedActivity = act;
                    RemoveReservedItems(day, i, duration);
                    PlannedWeek[day][i].ItemHeight = CalculateHeight(duration);

                    // jeśli aktywność jest zaplanowana, ale jest z poprzednich dni, to dodatkowo jej datę i godzinę oznacz kolorem czerwonym
                    if (PlannedWeek[day][i].ProposedActivity.StartDate < tempToday)
                    {
                        PlannedWeek[day][i].DateColor = "Red";
                        PlannedWeek[day][i].HourColor = "Red";
                    }

                    // po dodaniu aktywności, oznacz ją w localdb jako dodaną
                    LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);

                    if (act.StartDate == tempToday)
                        UsedHours[day] += duration;
                }
                // co daje krok 3b, 4b, 5b, 6b: aktywności niezaplanowane na aktualnie przetwarzany dzień zostaną zaproponowane na planie dnia w odpowiednim porządku

                // krok 3c
                // ten krok jest tylko dla aktywności zaplanowanych na aktualnie przetwarzany dzień, niewykonanych aktywności, ale BEZ ustalonej godziny
                // jeśli aktywność zostanie zaproponowana później, niż koniec dnia - wstaw ją
                // pamiętaj, że jeśli nie jest to aktywność zaplanowana na dzisiaj - nie wstawiaj jej
                // aktywność, która zostanie wtedy zaproponowana po północy - traktuj mimo wszystko jako dzisiejszą
                // ale nie dodawaj aktywności w ogóle, jeśli przekroczy tzw. "limit dnia" - np. godzinę 3:00 w nocy
                else if (act.StartDate == tempToday && GetTemporaryPlannedActivityId(day, i) + duration - 1 < DayLimit)
                {
                    PlannedWeek[day].Add(new PlannedActivity((PlannedWeek[day][i].Id + GetPreviousActivityDuration(day, i)) % 24, act));
                    PlannedWeek[day][i + 1].ItemHeight = CalculateHeight(duration);
                    // po dodaniu aktywności, oznacz ją w localdb jako dodaną
                    LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET IsAdded = 1 WHERE Id = " + act.Id);
                    UsedHours[day] += duration;
                }
                // co daje krok 3c: aktywności bez godziny, ale zaplanowane na aktualnie przetwarzany dzień, będą mogły znaleźć się na planie dnia
                // nawet, jeśli zostaną zaproponowane później, niż koniec dnia
            }


            // KROK 7: Pomiń listę "Nowe" - takie aktywności należy najpierw przejrzeć i zaplanować lub umieścić na jakiejś liście
            // KROK 8: Pomiń listę "Oddelegowane" - takie aktywności ktoś musi wykonać za użytkownika

            UsedHours[day] -= GetUsedPlacesInWorkingHours(day);
            UsedWorkingHours[day] = GetUsedPlacesInWorkingHours(day);
        }

        #region Filling Planned Day Helpers

        private bool AreAllRequiredPlacesEmpty(int day, int i, int duration)
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

        private bool AreAllRequiredHoursProper(int day, int i, int duration)
        {
            // sprawdź, czy pierwsza godzina dla wstawianej aktywności jest pusta w planie dnia
            if (PlannedWeek[day][i].ProposedActivity != null)
                return false;
            // sprawdź, czy aktywność nie będzie zaczynać się w godzinach pracy
            if (PlannedWeek[day][i].Id >= StartWork[day] && PlannedWeek[day][i].Id < EndWork[day])
                return false;
            // sprawdź, czy aktywność nie będzie kończyła się po rozpoczęciu pracy
            if (PlannedWeek[day][i].Id < StartWork[day] && StartWork[day] < (PlannedWeek[day][i].Id + duration))
                return false;
            else
                return true;
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

            if (act.EstimationId > 1)
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

        private int GetUsedPlacesInWorkingHours(int day)
        {
            int emptyPlacesInWorkingHours = 0;
            foreach (var act in PlannedWeek[day])
            {
                if (act.Id >= StartWork[day] && act.Id < EndWork[day] && act.ProposedActivity == null)
                    emptyPlacesInWorkingHours++;
            }

            return EndWork[day] - StartWork[day] - emptyPlacesInWorkingHours;
        }

        private int CalculateHeight(int duration)
        {
            return 55 + 40 * (duration - 1);
        }

        #endregion
    }

    class CompletelyUnhandledActivityException : Exception
    {
        public CompletelyUnhandledActivityException(string message) : base(message) { }
    }
}
