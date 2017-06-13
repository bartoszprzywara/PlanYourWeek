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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace PlanYourWeek
{
    public sealed partial class EditGenericProperty : Page
    {
        public EditGenericProperty()
        {
            this.InitializeComponent();
            SetUpPageAnimation();
        }

        public void SetUpPageAnimation()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;
            complexPropertyName = e.Parameter.ToString().Split('|')[0];
            int itemId = int.Parse(e.Parameter.ToString().Split('|')[1]);

            if (complexPropertyName == "Kontekst")
            {
                complexPropertyType = "Context";
                item = LocalDatabaseHelper.ReadItem<Context>(itemId);
            }
            if (complexPropertyName == "Projekt")
            {
                complexPropertyType = "Project";
                item = LocalDatabaseHelper.ReadItem<Project>(itemId);
            }

            var complexPropertyNameForTitle = LocalizedStrings.GetString("ActivityGeneric_Property_" + complexPropertyType + "/Text");
            NameTextBlock.Text = LocalizedStrings.GetString("Edit_GenericProperty_Edit/Text") + " " + complexPropertyNameForTitle.ToLower();
            NameTextBox.Text = item.Name;
        }

        private string complexPropertyName;
        private string complexPropertyType;
        private ComplexProperty item;

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        async private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Na pewno chcesz bezpowrotnie usunąć ten " + complexPropertyName.ToLower() + "? Wszystkie zadania mające ten " + complexPropertyName.ToLower() + " - stracą go", "Na pewno?");
            dialog.Commands.Add(new UICommand("Tak") { Id = 0 });
            dialog.Commands.Add(new UICommand("Nie") { Id = 1 });
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();

            if ((int)result.Id == 0)
            {
                LocalDatabaseHelper.ExecuteQuery("DELETE FROM " + complexPropertyType + " WHERE Id = " + item.Id);
                LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET " + complexPropertyType + "Id = NULL WHERE " + complexPropertyType + "Id = " + item.Id);
                App.PlannedWeekNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        async private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int counter = new ObservableCollection<ComplexProperty>(LocalDatabaseHelper.conn.Query<ComplexProperty>("SELECT * FROM " + complexPropertyType).Where(v => v.Name.ToLower() == NameTextBox.Text.ToLower()).ToList()).Count;

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                await (new MessageDialog(complexPropertyName + " musi mieć nazwę", "Nie da rady")).ShowAsync();
            else if (counter > 0 && NameTextBox.Text.ToLower() != item.Name.ToLower())
                await (new MessageDialog("Taki " + complexPropertyName.ToLower() + " już istnieje", "Nie da rady")).ShowAsync();
            else
            {
                LocalDatabaseHelper.ExecuteQuery("UPDATE " + complexPropertyType + " SET Name = '" + NameTextBox.Text + "' WHERE Id = " + item.Id);
                App.PlannedWeekNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }
    }
}
