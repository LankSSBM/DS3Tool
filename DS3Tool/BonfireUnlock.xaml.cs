using DS3Tool.services;
using DS3Tool.templates;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using static DS3Tool.MainWindow;
using static DS3Tool.services.BonfireService;

namespace DS3Tool
{
    /// <summary>
    /// Interaction logic for BonfireUnlock.xaml
    /// </summary>
    public partial class BonfireUnlock : Window
    {
        BonfireService _bonfireService;

        public BonfireUnlock(DS3Process process, Dictionary<string, BonfireLocation> bonfireDict)
        {
            _bonfireService = new BonfireService(process, bonfireDict);

            InitializeComponent();
            populate();
        }

        private void populate()
        {
            BonfireDropdown.Items.Clear();
            BonfireDropdown.ItemsSource = _bonfireService._bonfireDict.Keys;
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
