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
                var dialog = new MessageDialog("Projekt musi mieć nazwę");
                await dialog.ShowAsync();
            }
            else {
                LocalDatabaseHelper.InsertItem(new Project(AddItemTextBlock.Text));
                listofProjects.Add(LocalDatabaseHelper.ReadLastItem<Project>());
                AddItemTextBlock.Text = "";
            }
        }
    }
}
