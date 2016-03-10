using Remonty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Helpers
{
    public class LocalDatabaseHelper
    {
        private static readonly string sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");

        public static void InitializeLocalDB()
        {
            CreateDatabase();
#if DEBUG
            DeleteAllItemsInTable<Activity>();
            DeleteAllItemsInTable<Context>();
            DeleteAllItemsInTable<Project>();

            InsertItem(new Activity("Kupić farbę", "Biała 10l, Zielona 5l", "Wysoki", true, null, null, new DateTime(2016, 04, 21), null, "2 godz", "Zakupy", null));
            InsertItem(new Activity("Tytuł zadania 1", "Opis zadania 1", "Niski", false, new DateTime(2016, 03, 29), new TimeSpan(17, 34, 56), new DateTime(2016, 03, 30), new TimeSpan(19, 27, 44), "1 godz", "Spotkanie", "Położyć panele"));
            InsertItem(new Activity("Pomalować kuchnię", "Na zielono", "Normalny", false, new DateTime(2016, 04, 23), new TimeSpan(16, 00, 00), new DateTime(2016, 04, 23), new TimeSpan(20, 00, 00), "4 godz", "Kuchnia", "Pomalować mieszkanie"));
            //InsertItem(new Activity(null, null, null, null, null, null, null, null, null, null, null));

            string[] contexts = { "Zakupy", "Spotkanie", "Telefon", "Komputer", "Kuchnia", "Łazienka", "Przedpokój", "Salon", "Sypialnia" };
            foreach (string value in contexts)
                InsertItem(new Context(value));

            string[] projects = { "Pomalować mieszkanie", "Wymienić kaloryfery", "Położyć panele", "<Dodaj nowy>" };
            foreach (string value in projects)
                InsertItem(new Project(value));
#endif
        }

        public static void CreateDatabase()
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.CreateTable<Activity>();
                conn.CreateTable<Context>();
                conn.CreateTable<Project>();
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

        public static List<T> ReadAllItemsFromTable<T>() where T : class
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                List<T> myCollection = conn.Table<T>().ToList();
                //ObservableCollection<Activity> ActivitiesList = new ObservableCollection<Activity>(myCollection);
                return myCollection;
            }
        }

        public static List<string> ReadNamesFromTable<T>() where T : class, IHasName
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                List<T> tempCollection = conn.Table<T>().ToList();
                List<string> myCollection = new List<string>();
                foreach (T item in tempCollection)
                    myCollection.Add(item.Name);
                return myCollection;
            }
        }

        /*
        public static void UpdateNameInTable<T>(int Id, T editedName) where T : class, IHasName
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                var existingActivity = conn.Query<Activity>("SELECT * FROM Activity WHERE Id =" + Id.ToString()).FirstOrDefault();
                if (existingActivity != null)
                {
                    existingActivity.Title = editedActivity.Title;
                    existingActivity.Description = editedActivity.Description;
                    existingActivity.Priority = editedActivity.Priority;
                    existingActivity.IsAllDay = editedActivity.IsAllDay;
                    existingActivity.StartHour = editedActivity.StartHour;
                    existingActivity.StartDate = editedActivity.StartDate;
                    existingActivity.EndDate = editedActivity.EndDate;
                    existingActivity.Estimation = editedActivity.Estimation;
                    existingActivity.Context = editedActivity.Context;
                    existingActivity.Project = editedActivity.Project;

                    conn.RunInTransaction(() =>
                    {
                        conn.Update(existingActivity);
                    });
                }
            }
        }
        */

        public static void UpdateActivity(int Id, Activity editedActivity)
        {
            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                var existingActivity = conn.Query<Activity>("SELECT * FROM Activity WHERE Id =" + Id.ToString()).FirstOrDefault();
                if (existingActivity != null)
                {
                    existingActivity.Title = editedActivity.Title;
                    existingActivity.Description = editedActivity.Description;
                    existingActivity.Priority = editedActivity.Priority;
                    existingActivity.IsAllDay = editedActivity.IsAllDay;
                    existingActivity.StartHour = editedActivity.StartHour;
                    existingActivity.StartDate = editedActivity.StartDate;
                    existingActivity.EndDate = editedActivity.EndDate;
                    existingActivity.Estimation = editedActivity.Estimation;
                    existingActivity.Context = editedActivity.Context;
                    existingActivity.Project = editedActivity.Project;

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