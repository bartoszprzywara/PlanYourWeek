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
        public void CreateDatabase()
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.CreateTable<Activity>();
            }
        }

        public void Insert(Activity objActivity)
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.RunInTransaction(() =>
                {
                    conn.Insert(objActivity);
                });
            }
        }

        public Activity ReadContact(int activityId)
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                var existingActivity = conn.Query<Activity>("SELECT * FROM Activity WHERE Id =" + activityId).FirstOrDefault();
                return existingActivity;
            }
        }

        public List<Activity> ReadAllActivities()
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                List<Activity> myCollection = conn.Table<Activity>().ToList<Activity>();
                //ObservableCollection<Activity> ActivitiesList = new ObservableCollection<Activity>(myCollection);
                return myCollection;
            }
        }

        public void UpdateDetails(int Id, Activity editedActivity)
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");

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

        public void DeleteAllContact()
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.DropTable<Activity>();
                conn.CreateTable<Activity>();
                conn.Dispose();
                conn.Close();
            }
        }

        public void DeleteActivity(int Id)
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "localdb.sqlite");

            using (var conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                var existingActivity = conn.Query<Activity>("SELECT * FROM Activity WHERE Id =" + Id).FirstOrDefault();
                if (existingActivity != null)
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Delete(existingActivity);
                    });
                }
            }
        }
    }
}