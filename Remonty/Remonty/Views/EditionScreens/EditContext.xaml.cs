using Remonty.Helpers;
using Remonty.Models;
using System;
using System.Collections.Generic;
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

namespace Remonty
{
    public sealed partial class EditContext : Page
    {
        public EditContext()
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

        private Context context;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;
            context = e.Parameter as Context;

            NameTextBlock.Text = "Edytuj kontekst";
            NameTextBox.Text = context.Name;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        async private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Na pewno chcesz bezpowrotnie usunąć ten kontekst? Wszystkie zadania mające ten kontekst - stracą go", "Na pewno?");
            dialog.Commands.Add(new UICommand("Tak") { Id = 0 });
            dialog.Commands.Add(new UICommand("Nie") { Id = 1 });
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();

            if ((int)result.Id == 0)
            {
                LocalDatabaseHelper.DeleteItem<Context>(context.Id);
                LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET ContextId = NULL WHERE ContextID = " + context.Id);
                App.PlannedWeekNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        async private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int counter = LocalDatabaseHelper.CountItems<Context>("SELECT * FROM Context WHERE Name = '" + NameTextBox.Text + "' COLLATE NOCASE");

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                var dialog = new MessageDialog("Kontekst musi mieć nazwę", "Nie da rady");
                await dialog.ShowAsync();
            }
            else if (counter > 0 && NameTextBox.Text.ToLower() != context.Name.ToLower())
            {
                var dialog = new MessageDialog("Taki kontekst już istnieje", "Nie da rady");
                await dialog.ShowAsync();
            }
            else {
                LocalDatabaseHelper.UpdateNameInTable<Context>(context.Id, NameTextBox.Text);
                App.PlannedWeekNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }
    }
}
