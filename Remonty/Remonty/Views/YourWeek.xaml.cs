using Remonty.Helpers;
using Remonty.Models;
using System;
using System.Collections.Generic;
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

            // TODO: helper do bazy danych zrobic na statycznych metodach
            // TODO: wrzucic tam metode initialize db z InitializeLocalDatabase.cs
            // TODO: nazwac LocalDbHelper lub podobnei
            LocalDatabaseHelper readDB = new LocalDatabaseHelper();
            listofActivities = readDB.ReadAllActivities();
        }

        private List<Activity> listofActivities;

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedActivity = (Activity)e.ClickedItem;

            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(AddEditActivity), selectedActivity);
        }
    }
}
