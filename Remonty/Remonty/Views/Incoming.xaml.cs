using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Remonty
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Incoming : Page
    {
        public Incoming()
        {
            this.InitializeComponent();
        }

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private void createDBmine_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");

            using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path))
            {
                conn.CreateTable<User>();
            }
        }

        /*
        private void createDB_Click(object sender, RoutedEventArgs e)
        {
            LocalDatabaseHelper createDB = new LocalDatabaseHelper();
            createDB.CreateDatabase();
        }

        private void UpdateDB_Click(object sender, RoutedEventArgs e)
        {
            LocalDatabaseHelper update = new LocalDatabaseHelper();
            update.UpdateDetails("Suresh");
        }

        private void ReadDB_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<LocalDatabaseHelper.Students> listofStudents = new ObservableCollection<LocalDatabaseHelper.Students>();
            LocalDatabaseHelper readall = new LocalDatabaseHelper();
            listofStudents = readall.ReadAllStudents();
            studentListBox.ItemsSource = listofStudents.ToList();
        }

        private void DeleteDB_Click(object sender, RoutedEventArgs e)
        {
            LocalDatabaseHelper delete = new LocalDatabaseHelper();
            delete.DeleteAllContact();
        }

        private void createDBmine_Click(object sender, RoutedEventArgs e)
        {
            LocalDatabaseHelper createDB = new LocalDatabaseHelper();
            createDB.createTable();
        }
        */
    }
}
