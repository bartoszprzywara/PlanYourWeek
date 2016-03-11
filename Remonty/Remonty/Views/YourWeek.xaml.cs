using Remonty.Helpers;
using Remonty.Models;
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

namespace Remonty
{
    public sealed partial class YourWeek : Page
    {
        public YourWeek()
        {
            this.InitializeComponent();
            listofActivities = LocalDatabaseHelper.ReadAllItemsFromTable<Activity>();
        }

        private ObservableCollection<Activity> listofActivities;

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedActivity = (Activity)e.ClickedItem;

            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(AddEditActivity), selectedActivity);
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            int ItemID = (int)((FrameworkElement)e.OriginalSource).DataContext;
            int ItemIndex = LocalDatabaseHelper.ReadItemIndex<Activity>("Id", ItemID);

            listofActivities.RemoveAt(ItemIndex);
            LocalDatabaseHelper.DeleteItem<Activity>(ItemID);
        }
    }
}
