using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SQLite.Net;
using System.IO;
using System.Diagnostics;

namespace Remonty
{
    public class LocalDatabaseHelper
    {
        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public void createTable()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            using (var connection = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path))
            {
                connection.CreateTable<User>();
                Debug.WriteLine("wejszło i zrobiło");
            }
        }

        public void CreateDatabase()
        {
            //CreateTablesResult

            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Studentdb.sqlite");

            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.CreateTable<Students>();

            }
        }
        public static void Insert(Students objContact)
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Studentdb.sqlite");

            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                conn.RunInTransaction(() =>
                {
                    conn.Insert(objContact);
                });
            }
        }
        // Retrieve the specific contact from the database.   
        public Students ReadContact(int contactid)
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Studentdb.sqlite");

            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                var existingconact = conn.Query<Students>("select * from Students where Id =" + contactid).FirstOrDefault();
                return existingconact;
            }
        }
        public ObservableCollection<Students> ReadAllStudents()
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Studentdb.sqlite");

            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {
                List<Students> myCollection = conn.Table<Students>().ToList<Students>();
                ObservableCollection<Students> ContactsList = new ObservableCollection<Students>(myCollection);
                return ContactsList;
            }

        }
        public void UpdateDetails(string name)
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Studentdb.sqlite");

            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {

                var existingconact = conn.Query<Students>("select * from Students where Name =" + name).FirstOrDefault();
                if (existingconact != null)
                {
                    existingconact.Name = name;
                    existingconact.Address = "NewAddress";
                    existingconact.Mobile = "962623233";
                    conn.RunInTransaction(() =>
                    {
                        conn.Update(existingconact);
                    });
                }

            }
        }
        //Delete all contactlist or delete Contacts table   
        public void DeleteAllContact()
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Studentdb.sqlite");

            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {

                conn.DropTable<Students>();
                conn.CreateTable<Students>();
                conn.Dispose();
                conn.Close();

            }
        }
        //Delete specific contact   
        public void DeleteContact(int Id)
        {
            var sqlpath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Studentdb.sqlite");

            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlpath))
            {

                var existingconact = conn.Query<Students>("select * from Students where Id =" + Id).FirstOrDefault();
                if (existingconact != null)
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Delete(existingconact);
                    });
                }
            }
        }
        public class Students
        {
            [SQLite.Net.Attributes.PrimaryKey, SQLite.Net.Attributes.AutoIncrement]
            public int Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string Mobile { get; set; }
            public Students()
            {

            }
            public Students(string name, string address, string mobile)
            {
                Name = name;
                Address = address;
                Mobile = mobile;
            }
        }

    }
}