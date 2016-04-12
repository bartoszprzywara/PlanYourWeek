using Remonty.Models;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Helpers
{
    public class LocalDatabaseHelper
    {
        private static string sqlpath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");
        private static SQLiteConnectionString sqlconnstring = new SQLiteConnectionString(sqlpath, true);
        private static SQLiteConnectionWithLock _conn;

        public static SQLiteConnectionWithLock conn => _conn ??
            (_conn = new SQLiteConnectionWithLock(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlconnstring));

        public static void InitializeLocalDB()
        {
            if (!File.Exists(sqlpath))
            {
                CreateDatabase();

                DeleteAllItemsInTable<Activity>();
                DeleteAllItemsInTable<Context>();
                DeleteAllItemsInTable<Project>();
                DeleteAllItemsInTable<Estimation>();
                DeleteAllItemsInTable<Priority>();
                DeleteAllItemsInTable<Settings>();
//#if DEBUG
                // konstruktor: tytuł, opis, prior, calydzien, lista, start, godz, end, godz, est, kont, proj
                // lista: Nowe, Zaplanowane, Najblizsze, Kiedys, Oddelegowane
                // est: 1-"15 min", 2-"30 min", 3-"1 godz", 4-"2 godz", 5-"3 godz", 6-"4 godz", 7-"6 godz", 8-"10 godz"
                // kont: 1-"Zakupy", 2-"Spotkanie", 3-"Telefon", 4-"Prace", 5-"Komputer", 6-"Kuchnia", 7-"Łazienka", 8-"Przedpokój", 9-"Salon", 10-"Sypialnia"
                // proj: 1-"Malowanie", 2-"Kaloryfery", 3-"Podłogi", 4-"Okna"

                InsertItem(new Activity("Opróżnić piwnicę", "Zalegają jakieś garty", 1, true, "Najblizsze", null, null, null, null, 4, 4, null));
                InsertItem(new Activity("Wymienić zamki", "", 3, false, "Zaplanowane", DateTime.Today.AddDays(-5), new TimeSpan(19, 00, 00), null, null, 2, 8, null));
                ExecuteQuery("UPDATE Activity SET IsDone = 1");

                InsertItem(new Activity("Pomyśleć nad kuchenką", "Czy kupić nową kuchenkę?", 2, true, "Nowe", null, null, null, null, null, 6, null));
                InsertItem(new Activity("Ogłoszenie o wymianie okien", "Na klatce wisi info", 3, true, "Nowe", null, null, DateTime.Today.AddDays(12), null, null, null, 4));
                InsertItem(new Activity("Pomalować kuchnię", "Na zielono", 2, false, "Zaplanowane", DateTime.Today.AddDays(1), new TimeSpan(16, 00, 00), DateTime.Today.AddDays(9), new TimeSpan(21, 00, 00), 5, 6, 1));
                InsertItem(new Activity("Kupić płytki", "Do kuchni i łazienki", 2, true, "Zaplanowane", DateTime.Today.AddDays(2), null, DateTime.Today.AddDays(11), null, 4, 1, 3));
                InsertItem(new Activity("Mają przywieźć kanapę", "Zadzwonią godzinę przed", 2, true, "Zaplanowane", DateTime.Today.AddDays(3), null, null, null, 3, 9, null));
                InsertItem(new Activity("Cyklinowanie podłogi", "Przyjdzie Pan Karol z asystentem", 2, false, "Zaplanowane", DateTime.Today, new TimeSpan(10, 30, 00), null, null, 5, 4, 3));
                InsertItem(new Activity("Pomiar gazu", "Przyjdzie Pan Jan Kowalski", 1, false, "Zaplanowane", DateTime.Today, new TimeSpan(08, 00, 00), DateTime.Today, new TimeSpan(09, 00, 00), 1, 2, null));
                InsertItem(new Activity("Kupić farbę", "Biała 10l, zielona 5l", 2, true, "Najblizsze", null, null, DateTime.Today.AddDays(2), null, 4, 1, 1));
                InsertItem(new Activity("Zmierzyć, ile trzeba płytek", "Kuchnia i łazienka", 3, true, "Najblizsze", null, null, DateTime.Today.AddDays(1), null, 2, 4, 3));
                InsertItem(new Activity("Kupić okna", "", 3, true, "Najblizsze", null, null, DateTime.Today.AddDays(4), null, 5, 1, 4));
                InsertItem(new Activity("Znaleźć ekipę do wymiany okien", "Takiej, która zrobi to najszybciej", 3, true, "Najblizsze", null, null, DateTime.Today.AddDays(3), null, 3, 5, 4));
                InsertItem(new Activity("Przesunąć lampę na suficie", "Bliżej aneksu", 2, true, "Najblizsze", null, null, null, null, 4, 9, null));
                InsertItem(new Activity("Powiesić szafkę w łazience", "Nad pralką", 1, true, "Najblizsze", null, null, DateTime.Today.AddDays(3), null, 3, 7, null));
                InsertItem(new Activity("Sprawdzić kod do domofonu", "W spółdzielni", 1, true, "Najblizsze", null, null, null, null, null, 2, null));
                InsertItem(new Activity("Listwa na złączeniu", "Między salonem a aneksem", 1, true, "Kiedys", null, null, null, null, 2, 4, 3));
                InsertItem(new Activity("Odkamienić pralkę", "", 2, true, "Kiedys", null, null, DateTime.Today.AddDays(18), null, 4, 7, null));
                InsertItem(new Activity("Zasłony do okien", "Tata ma przywieźć od babci", 1, true, "Oddelegowane", null, null, DateTime.Today.AddDays(10), null, null, 1, 4));
                InsertItem(new Activity("Założenie rolet na okna", "Fachowiec ma zadzwonić jak się dowie", 2, false, "Oddelegowane", null, null, DateTime.Today.AddDays(6), new TimeSpan(14, 00, 00), 1, 10, 4));
                //InsertItem(new Activity(null, null, 2, true, null, null, null, null, null, null, null, null));

                string[] projects = { "Malowanie", "Kaloryfery", "Podłogi", "Okna" };
                foreach (string value in projects)
                    InsertItem(new Project(value));
//#endif
                string[] contexts = { "Zakupy", "Spotkanie", "Telefon", "Prace", "Komputer", "Kuchnia", "Łazienka", "Przedpokój", "Salon", "Sypialnia" };
                foreach (string value in contexts)
                    InsertItem(new Context(value));

                string[] estimations = { "15 min", "30 min", "1 godz", "2 godz", "3 godz", "4 godz", "6 godz", "10 godz" };
                foreach (string value in estimations)
                    InsertItem(new Estimation(value));

                string[] priorities = { "Niski", "Normalny", "Wysoki" };
                foreach (string value in priorities)
                    InsertItem(new Priority(value));

                InsertItem(new Settings("StartDay", new TimeSpan(07, 00, 00).ToString()));
                InsertItem(new Settings("StartWorking", new TimeSpan(09, 00, 00).ToString()));
                InsertItem(new Settings("EndWorking", new TimeSpan(17, 00, 00).ToString()));
                InsertItem(new Settings("EndDay", new TimeSpan(23, 00, 00).ToString()));
                InsertItem(new Settings("WorkingHoursEnabled", true.ToString()));
            }
        }

        public static void CreateDatabase()
        {
            using (conn.Lock())
            {
                conn.CreateTable<Activity>();
                conn.CreateTable<Context>();
                conn.CreateTable<Project>();
                conn.CreateTable<Estimation>();
                conn.CreateTable<Priority>();
                conn.CreateTable<Settings>();
            }
        }

        public static void InsertItem<T>(T objItem) where T : class
        {
            using (conn.Lock())
            {
                conn.RunInTransaction(() =>
                {
                    conn.Insert(objItem);
                });
            }
        }

        public static T ReadItem<T>(int itemId) where T : class
        {
            using (conn.Lock())
            {
                var existingItem = conn.Query<T>("SELECT * FROM " + typeof(T).Name + " WHERE Id =" + itemId).FirstOrDefault();
                return existingItem;
            }
        }

        public static int ReadItemIndex<T>(string columnName, int itemId) where T : class
        {
            using (conn.Lock())
            {
                var itemIndex = conn.Query<T>("SELECT * FROM " + typeof(T).Name + " WHERE " + columnName + " <" + itemId).Count;
                return itemIndex;
            }
        }

        public static T ReadLastItem<T>() where T : class
        {
            using (conn.Lock())
            {
                var lastItem = conn.Query<T>("SELECT * FROM " + typeof(T).Name + " ORDER BY id DESC LIMIT 1").FirstOrDefault();
                return lastItem;
            }
        }

        public static ObservableCollection<T> ReadAllItemsFromTable<T>() where T : class
        {
            using (conn.Lock())
            {
                List<T> tempCollection = conn.Table<T>().ToList();
                ObservableCollection<T> myCollection = new ObservableCollection<T>(tempCollection);
                return myCollection;
            }
        }

        public static ObservableCollection<string> ReadNamesFromTable<T>() where T : class, IHasName
        {
            using (conn.Lock())
            {
                List<T> tempCollection = conn.Table<T>().ToList();
                ObservableCollection<string> myCollection = new ObservableCollection<string>();
                foreach (T item in tempCollection)
                    myCollection.Add(item.Name);
                return myCollection;
            }
        }

        public static void UpdateNameInTable<T>(int Id, string editedName) where T : class, IHasName
        {
            using (conn.Lock())
            {
                var existingItem = conn.Query<T>("SELECT * FROM " + typeof(T).Name + " WHERE Id =" + Id.ToString()).FirstOrDefault();
                if (existingItem != null)
                {
                    existingItem.Name = editedName;
                    conn.RunInTransaction(() =>
                    {
                        conn.Update(existingItem);
                    });
                }
            }
        }

        public static void UpdateActivity(int Id, Activity editedActivity)
        {
            using (conn.Lock())
            {
                var existingActivity = conn.Query<Activity>("SELECT * FROM Activity WHERE Id =" + Id.ToString()).FirstOrDefault();
                if (existingActivity != null)
                {
                    existingActivity.Title = editedActivity.Title;
                    existingActivity.Description = editedActivity.Description;
                    existingActivity.PriorityId = editedActivity.PriorityId;
                    existingActivity.IsAllDay = editedActivity.IsAllDay;
                    existingActivity.List = editedActivity.List;
                    existingActivity.StartDate = editedActivity.StartDate;
                    existingActivity.StartHour = editedActivity.StartHour;
                    existingActivity.EndDate = editedActivity.EndDate;
                    existingActivity.EndHour = editedActivity.EndHour;
                    existingActivity.EstimationId = editedActivity.EstimationId;
                    existingActivity.ContextId = editedActivity.ContextId;
                    existingActivity.ProjectId = editedActivity.ProjectId;

                    conn.RunInTransaction(() =>
                    {
                        conn.Update(existingActivity);
                    });
                }
            }
        }

        public static void DeleteAllItemsInTable<T>() where T : class
        {
            using (var conn = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.DropTable<T>();
                conn.CreateTable<T>();
                conn.Dispose();
                conn.Close();
            }
        }

        public static void DeleteItem<T>(int itemId) where T : class
        {
            using (conn.Lock())
            {
                var existingItem = conn.Query<T>("SELECT * FROM " + typeof(T).Name + " WHERE Id =" + itemId).FirstOrDefault();
                if (existingItem != null)
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Delete(existingItem);
                    });
                }
            }
        }

        public static void ExecuteQuery(string sqlquery)
        {
            using (conn.Lock())
                conn.Execute(sqlquery);
        }

        public static int CountItems<T>(string sqlquery) where T : class
        {
            using (conn.Lock())
            {
                var counter = conn.Query<T>(sqlquery).Count;
                return counter;
            }
        }
    }
}