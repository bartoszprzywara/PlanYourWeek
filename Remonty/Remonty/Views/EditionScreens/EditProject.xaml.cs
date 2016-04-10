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
    public sealed partial class EditProject : Page
    {
        public EditProject()
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

        private Project project;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;
            project = e.Parameter as Project;

            NameTextBlock.Text = "Kontekst " + project.Name;
            NameTextBox.Text = project.Name;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        async private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Na pewno chcesz bezpowrotnie usunąć ten kontekst? Wszystkie zadania mające ten kontekst stracą go", "Na pewno?");
            dialog.Commands.Add(new UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new UICommand("No") { Id = 1 });
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();

            if ((int)result.Id == 0)
            {
                LocalDatabaseHelper.DeleteItem<Project>(project.Id);
                LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET ProjectId = NULL WHERE ProjectID = " + project.Id);
                App.PlanNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        async private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int counter = LocalDatabaseHelper.CountItems<Project>("SELECT * FROM Project WHERE Name = '" + NameTextBox.Text + "' COLLATE NOCASE");

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                var dialog = new MessageDialog("Projekt musi mieć nazwę", "Nie da rady");
                await dialog.ShowAsync();
            }
            else if (counter > 0 && NameTextBox.Text.ToLower() != project.Name.ToLower())
            {
                var dialog = new MessageDialog("Taki projekt już istnieje", "Nie da rady");
                await dialog.ShowAsync();
            }
            else {
                LocalDatabaseHelper.UpdateNameInTable<Project>(project.Id, NameTextBox.Text);
                App.PlanNeedsToBeReloaded = true;

                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }
    }
}
