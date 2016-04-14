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
    public sealed partial class ActivityGenericProperty : Page
    {
        public ActivityGenericProperty()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;
            complexPropertyName = e.Parameter.ToString().Substring(0, e.Parameter.ToString().Length - 1);

            if (complexPropertyName == "Kontekst")
            {
                complexPropertyType = "Context";
                listOfItems = new ObservableCollection<ComplexProperty>(LocalDatabaseHelper.ReadAllItemsFromTable<Context>());
            }
            if (complexPropertyName == "Projekt")
            {
                complexPropertyType = "Project";
                listOfItems = new ObservableCollection<ComplexProperty>(LocalDatabaseHelper.ReadAllItemsFromTable<Project>());
            }

            foreach (var item in listOfItems)
                item.ActivitiesCounter = LocalDatabaseHelper.CountItems<Activity>("SELECT * FROM Activity WHERE " + complexPropertyType + "Id = " + item.Id);

            if (App.PlannedWeekNeedsToBeReloaded)
            {
                App.ReloadPlannedWeekTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    (new YourWeekPlanningHelper()).GetPlannedWeek();
                    App.PlannedWeekNeedsToBeReloaded = false;
                });
            }
        }

        private string complexPropertyName;
        private string complexPropertyType;
        private ObservableCollection<ComplexProperty> listOfItems;

        async private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            int counter = LocalDatabaseHelper.CountItems<ComplexProperty>("SELECT * FROM " + complexPropertyType + " WHERE Name = '" + AddItemTextBox.Text + "' COLLATE NOCASE");

            if (string.IsNullOrWhiteSpace(AddItemTextBox.Text))
            {
                var dialog = new MessageDialog(complexPropertyName + " musi mieć nazwę", "Nie da rady");
                await dialog.ShowAsync();
            }
            else if (counter > 0)
            {
                var dialog = new MessageDialog("Taki " + complexPropertyName.ToLower() + " już istnieje", "Nie da rady");
                await dialog.ShowAsync();
            }
            else {
                if (complexPropertyName == "Kontekst")
                {
                    LocalDatabaseHelper.InsertItem(new Context(AddItemTextBox.Text));
                    listOfItems.Add(LocalDatabaseHelper.ReadLastItem<Context>());
                }
                if (complexPropertyName == "Projekt")
                {
                    LocalDatabaseHelper.InsertItem(new Project(AddItemTextBox.Text));
                    listOfItems.Add(LocalDatabaseHelper.ReadLastItem<Project>());
                }
                AddItemTextBox.Text = string.Empty;
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (ComplexProperty)e.ClickedItem;

            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(EditGenericProperty), complexPropertyName + "|" + selectedItem.Id);
        }
    }
}
