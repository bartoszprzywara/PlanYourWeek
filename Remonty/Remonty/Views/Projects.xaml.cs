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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Remonty
{
    public sealed partial class Projects : Page
    {
        public Projects()
        {
            this.InitializeComponent();
            listofItems = LocalDatabaseHelper.ReadAllItemsFromTable<Project>();

            if (App.PlannedWeekNeedsToBeReloaded)
            {
                App.ReloadPlannedWeekTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    (new YourWeekPlanningHelper()).GetPlannedWeek();
                    App.PlannedWeekNeedsToBeReloaded = false;
                });
            }
        }

        private ObservableCollection<Project> listofItems;

        async private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            int counter = LocalDatabaseHelper.CountItems<Project>("SELECT * FROM Project WHERE Name = '" + AddItemTextBox.Text + "' COLLATE NOCASE");

            if (string.IsNullOrWhiteSpace(AddItemTextBox.Text))
            {
                var dialog = new MessageDialog("Projekt musi mieć nazwę", "Nie da rady");
                await dialog.ShowAsync();
            }
            else if (counter > 0)
            {
                var dialog = new MessageDialog("Taki projekt już istnieje", "Nie da rady");
                await dialog.ShowAsync();
            }
            else {
                LocalDatabaseHelper.InsertItem(new Project(AddItemTextBox.Text));
                listofItems.Add(LocalDatabaseHelper.ReadLastItem<Project>());
                AddItemTextBox.Text = string.Empty;
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedContext = (Project)e.ClickedItem;

            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(EditProject), selectedContext);
        }
    }
}
