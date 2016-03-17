using Remonty.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Helpers
{
    public class LocalDatabaseHelper
    {
        public static readonly string sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");

        public static void InitializeLocalDB()
        {
            CreateDatabase();
#if DEBUG
            DeleteAllItemsInTable<Activity>();
            DeleteAllItemsInTable<Context>();
            DeleteAllItemsInTable<Project>();
            DeleteAllItemsInTable<Estimation>();
            DeleteAllItemsInTable<Priority>();

            InsertItem(new Activity("Kupić farbę", "Biała 10l, Zielona 5l", 3, true, "Najblizsze", null, null, new DateTime(2016, 04, 21), null, 5, 1, null));
            InsertItem(new Activity("Tytuł zadania 1", "Opis zadania 1", 1, false, "Zaplanowane", new DateTime(2016, 03, 29), new TimeSpan(17, 34, 56), new DateTime(2016, 03, 30), new TimeSpan(19, 27, 44), 4, 2, 3));
            InsertItem(new Activity("Pomalować kuchnię", "Na zielono", 2, false, "Zaplanowane", new DateTime(2016, 04, 23), new TimeSpan(16, 00, 00), new DateTime(2016, 04, 23), new TimeSpan(20, 00, 00), 7, 5, 1));
            InsertItem(new Activity(null, null, null, null, null, null, null, null, null, null, null, null));

            string[] contexts = { "Zakupy", "Spotkanie", "Telefon", "Komputer", "Kuchnia", "Łazienka", "Przedpokój", "Salon", "Sypialnia" };
            foreach (string value in contexts)
                InsertItem(new Context(value));

            string[] projects = { "Pomalować mieszkanie", "Wymienić kaloryfery", "Położyć panele" };
            foreach (string value in projects)
                InsertItem(new Project(value));

            string[] estimations = { "15 min", "30 min", "1 godz", "2 godz", "3 godz", "4 godz", "6 godz", "10 godz" };
            foreach (string value in estimations)
                InsertItem(new Estimation(value));

            string[] priorities = { "Niski", "Normalny", "Wysoki" };
            foreach (string value in priorities)
                InsertItem(new Priority(value));
#endif
        }

        public static void CreateDatabase()
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.CreateTable<Activity>();
                conn.CreateTable<Context>();
                conn.CreateTable<Project>();
                conn.CreateTable<Estimation>();
                conn.CreateTable<Priority>();
            }
        }

        public static void InsertItem<T>(T objItem) where T : class
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.RunInTransaction(() =>
                {
                    conn.Insert(objItem);
                });
            }
        }

        public static T ReadItem<T>(int itemId) where T : class
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                var existingItem = conn.Query<T>("SELECT * FROM " + typeof(T).Name + " WHERE Id =" + itemId).FirstOrDefault();
                return existingItem;
            }
        }

        public static int ReadItemIndex<T>(string columnName, int itemId) where T : class
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                var itemIndex = conn.Query<T>("SELECT * FROM " + typeof(T).Name + " WHERE " + columnName + " <" + itemId).Count;
                return itemIndex;
            }
        }

        public static T ReadLastItem<T>() where T : class
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                var lastItem = conn.Query<T>("SELECT * FROM " + typeof(T).Name + " ORDER BY id DESC LIMIT 1").FirstOrDefault();
                return lastItem;
            }
        }

        public static ObservableCollection<T> ReadAllItemsFromTable<T>() where T : class
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                List<T> tempCollection = conn.Table<T>().ToList();
                ObservableCollection<T> myCollection = new ObservableCollection<T>(tempCollection);
                return myCollection;
            }
        }

        public static ObservableCollection<string> ReadNamesFromTable<T>() where T : class, IHasName
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
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
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
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
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                var existingActivity = conn.Query<Activity>("SELECT * FROM Activity WHERE Id =" + Id.ToString()).FirstOrDefault();
                if (existingActivity != null)
                {
                    existingActivity.Title = editedActivity.Title;
                    existingActivity.Description = editedActivity.Description;
                    existingActivity.PriorityId = editedActivity.PriorityId;
                    existingActivity.IsAllDay = editedActivity.IsAllDay;
                    existingActivity.List = editedActivity.List;
                    existingActivity.StartHour = editedActivity.StartHour;
                    existingActivity.StartDate = editedActivity.StartDate;
                    existingActivity.EndDate = editedActivity.EndDate;
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
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.DropTable<T>();
                conn.CreateTable<T>();
                conn.Dispose();
                conn.Close();
            }
        }

        public static void DeleteItem<T>(int itemId) where T : class
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
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
    }
}