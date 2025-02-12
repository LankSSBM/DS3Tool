using DS3Tool.services;
using DS3Tool.templates;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using static DS3Tool.MainWindow;

namespace DS3Tool
{
    /// <summary>
    /// Interaction logic for BonfireUnlock.xaml
    /// </summary>
    public partial class BonfireUnlock : Window
    {
        DS3Process _process;
        BonfireService _bonfireService;
        Dictionary<string, Item> _itemDict;
        List<LoadoutTemplate> Templates;

        public BonfireUnlock(DS3Process process)
        {
            _process = process;
            _bonfireService = new BonfireService(process);

            InitializeComponent();
            populate();
        }

        private void populate()
        {
            BonfireDropdown.Items.Clear();
            BonfireDropdown.ItemsSource = _bonfireService.BonfireLocations.Keys;
            BonfireDropdown.SelectedIndex = 0;
        }

        private void UnlockSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedBonfire = BonfireDropdown.SelectedItem != null ? BonfireDropdown.SelectedItem.ToString() : null;
            if (!string.IsNullOrEmpty(selectedBonfire))
            {
                _bonfireService.unlockBonfire(selectedBonfire);
                MessageBox.Show("Bonfire Unlocked.");
            }
            else
            {
                MessageBox.Show("Please select a bonfire location.");
            }
        }

        private void UnlockAllButton_Click(object sender, RoutedEventArgs e)
        {
            _bonfireService.unlockAllBonfires();
            MessageBox.Show("All Bonfires Unlocked.");
        }
    }
}
