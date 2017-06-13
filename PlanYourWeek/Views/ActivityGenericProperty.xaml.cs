using PlanYourWeek.Helpers;
using PlanYourWeek.Models;
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

namespace PlanYourWeek
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
            else if (complexPropertyName == "Projekt")
            {
                complexPropertyType = "Project";
                listOfItems = new ObservableCollection<ComplexProperty>(LocalDatabaseHelper.ReadAllItemsFromTable<Project>());
            }

            foreach (var item in listOfItems)
                item.ActivitiesCounter = LocalDatabaseHelper.CountItems<Activity>("SELECT * FROM Activity WHERE " + complexPropertyType + "Id = " + item.Id);

            if (App.PlannedWeekNeedsToBeReloaded)
            {
                App.ReloadPlannedWeekTask?.Wait();
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
            int counter = new ObservableCollection<ComplexProperty>(LocalDatabaseHelper.conn.Query<ComplexProperty>("SELECT * FROM " + complexPropertyType).Where(v => v.Name.ToLower() == AddItemTextBox.Text.ToLower()).ToList()).Count;
            var complexPropertyNameForAlert = LocalizedStrings.GetString("ActivityGeneric_Property_" + complexPropertyType + "/Text");

            if (string.IsNullOrWhiteSpace(AddItemTextBox.Text))
            {
                var alertNameTitle = LocalizedStrings.GetString("ActivityGeneric_Property_Alert_PropertyHasToHaveName_Title/Text");
                var alertNameContentEnd = LocalizedStrings.GetString("ActivityGeneric_Property_Alert_PropertyHasToHaveName_Content_End/Text");

                await (new MessageDialog(complexPropertyNameForAlert + " " + alertNameContentEnd, alertNameTitle)).ShowAsync();
            }
            else if (counter > 0)
            {
                var alertExistsTitle = LocalizedStrings.GetString("ActivityGeneric_Property_Alert_PropertyAlreadyExists_Title/Text");
                var alertExistsContentStart = LocalizedStrings.GetString("ActivityGeneric_Property_Alert_PropertyAlreadyExists_Content_Start/Text");
                var alertExistsContentEnd = LocalizedStrings.GetString("ActivityGeneric_Property_Alert_PropertyAlreadyExists_Content_End/Text");

                await (new MessageDialog(alertExistsContentStart + " " + complexPropertyNameForAlert.ToLower() + " " + alertExistsContentEnd, alertExistsTitle)).ShowAsync();
            }
            else
            {
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
