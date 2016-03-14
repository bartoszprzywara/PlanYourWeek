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
            listofProjects = LocalDatabaseHelper.ReadAllItemsFromTable<Project>();
        }

        private ObservableCollection<Project> listofProjects;

        async private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddItemTextBlock.Text == "")
            {
                var dialog = new MessageDialog("Projekt musi mieć nazwę", "Nie da rady");
                await dialog.ShowAsync();
            }
            else {
                LocalDatabaseHelper.InsertItem(new Project(AddItemTextBlock.Text));
                listofProjects.Add(LocalDatabaseHelper.ReadLastItem<Project>());
                AddItemTextBlock.Text = "";
            }
        }

        async private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //var ItemID = ((FrameworkElement)e.OriginalSource).DataContext;
            //var dialog = new MessageDialog(ItemID.ToString());
            //await dialog.ShowAsync();
            //http://www.familie-smits.com/development-tips/tinder-control-for-your-windows-app


            EditItemContentDialog contentDialog = new EditItemContentDialog();
            contentDialog.MinWidth = this.ActualWidth;
            contentDialog.MaxWidth = this.ActualWidth;
            ContentDialogResult result = await contentDialog.ShowAsync();

            /*
            var dialog = new ContentDialog()
            {
                Title = "Lorem Ipsum",
                MaxWidth = this.ActualWidth, // Required for Mobile!
                Content = EditItemContentDialog
            };

            dialog.PrimaryButtonText = "OK";
            dialog.IsPrimaryButtonEnabled = true;
            dialog.PrimaryButtonClick += delegate
            {
            };

            var result = await dialog.ShowAsync();

            /*
            var btn = sender as Button;
            var dialog = new ContentDialog()
            {
                Title = "Lorem Ipsum",
                //RequestedTheme = ElementTheme.Dark,
                //FullSizeDesired = true,
                MaxWidth = this.ActualWidth // Required for Mobile!
            };

            // Setup Content
            var panel = new StackPanel();

            panel.Children.Add(new TextBlock
            {
                Text = "Aliquam laoreet magna sit amet mauris iaculis ornare. " +
                            "Morbi iaculis augue vel elementum volutpat.",
                TextWrapping = TextWrapping.Wrap,
            });

            var cb = new CheckBox
            {
                Content = "Agree"
            };

            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding
            {
                Source = dialog,
                Path = new PropertyPath("IsPrimaryButtonEnabled"),
                Mode = BindingMode.TwoWay,
            });

            panel.Children.Add(cb);
            dialog.Content = panel;

            // Add Buttons
            dialog.PrimaryButtonText = "OK";
            dialog.IsPrimaryButtonEnabled = false;
            dialog.PrimaryButtonClick += delegate {
                btn.Content = "Result: OK";
            };

            dialog.SecondaryButtonText = "Cancel";
            dialog.SecondaryButtonClick += delegate {
                btn.Content = "Result: Cancel";
            };

            // Show Dialog
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.None)
            {
                btn.Content = "Result: NONE";
            }
            */
        }
    }
}
