using DS3Tool.services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static DS3Tool.MainWindow;

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
            BonfireDropdown.ItemsSource = _bonfireService._bonfireDict.Keys.Skip(1);
            BonfireDropdown.SelectedIndex = 0;
        }

        private void WarpToSelectedButton_Click(object sender, RoutedEventArgs e)
        {

            string selectedBonfire = BonfireDropdown.SelectedItem != null ? BonfireDropdown.SelectedItem.ToString() : null;
            if (!string.IsNullOrEmpty(selectedBonfire))
            {
                _bonfireService.warpToBonfire(selectedBonfire);
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a bonfire location.");
            }
        }

        private void UnlockAllButton_Click(object sender, RoutedEventArgs e)
        {
            _bonfireService.unlockAllBonfires();

            if (MessageBox.Show("All Bonfires Unlocked.") == MessageBoxResult.OK)
            {
                this.Close();
            }
        }
    }
}
