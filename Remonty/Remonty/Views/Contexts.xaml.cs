﻿using Remonty.Helpers;
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
    public sealed partial class Contexts : Page
    {
        public Contexts()
        {
            this.InitializeComponent();
            listofItems = LocalDatabaseHelper.ReadAllItemsFromTable<Context>();
        }

        private ObservableCollection<Context> listofItems;

        async private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            int counter = LocalDatabaseHelper.CountItems<Context>("SELECT * FROM Context WHERE Name = '" + AddItemTextBox.Text + "' COLLATE NOCASE");

            if (AddItemTextBox.Text == "")
            {
                var dialog = new MessageDialog("Kontekst musi mieć nazwę", "Nie da rady");
                await dialog.ShowAsync();
            }
            else if (counter > 0)
            {
                var dialog = new MessageDialog("Taki kontekst już istnieje", "Nie da rady");
                await dialog.ShowAsync();
            }
            else {
                LocalDatabaseHelper.InsertItem(new Context(AddItemTextBox.Text));
                listofItems.Add(LocalDatabaseHelper.ReadLastItem<Context>());
                AddItemTextBox.Text = "";
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //var ItemID = ((FrameworkElement)e.OriginalSource).DataContext;
            var selectedContext = (Context)e.ClickedItem;

            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(EditContext), selectedContext);
        }
    }
}
