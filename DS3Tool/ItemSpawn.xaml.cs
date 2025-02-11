using DS3Tool.services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DS3Tool
{
    /// <summary>
    /// Interaction logic for ItemSpawn.xaml
    /// </summary>
    public partial class ItemSpawn : Window
    {
        DS3Process _process;
        ItemSpawnService _itemSpawnService;
        Dictionary<string, string> _itemDict;

        public ItemSpawn(DS3Process process, Dictionary<string, string> itemDict)
        {
            _process = process;
            _itemSpawnService = new ItemSpawnService(_process);
            _itemDict = itemDict;
            InitializeComponent();
            populate();
            //updateMatch();
        }
        void populate()
        {
            infusionTypeComboBox.Items.Clear();
            infusionTypeComboBox.ItemsSource = _itemSpawnService.INFUSION_TYPES.Keys;
            infusionTypeComboBox.SelectedIndex = 0;

            upgradeComboBox.Items.Clear();
            upgradeComboBox.ItemsSource = _itemSpawnService.UPGRADES.Keys;
            upgradeComboBox.SelectedIndex = 0;

            itemList.Items.Clear();
            VirtualizingPanel.SetIsVirtualizing(itemList, true);
            VirtualizingPanel.SetVirtualizationMode(itemList, VirtualizationMode.Recycling);

            foreach (KeyValuePair<string, string> item in _itemDict)
            {
                itemList.Items.Add(item.Key);
            }
        }

        string matchingItem = "";

        private void spawnItem(object sender, RoutedEventArgs e) {
            const string MIN_WEAPON_ID = "000D9490";
            const string MAX_WEAPON_ID = "015F1AD0";

            if (itemList.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to spawn.");
                return;
            }

            string selectedItem = itemList.SelectedItem.ToString();
            if (!_itemDict.TryGetValue(selectedItem, out string hexId))
            {
                MessageBox.Show("Item not found in dictionary.");
                return;
            }

            uint formattedId = uint.Parse(hexId, System.Globalization.NumberStyles.HexNumber);
            string selectedInfusion = infusionTypeComboBox.SelectedItem.ToString();
            string selectedUpgrade = upgradeComboBox.SelectedItem.ToString();
            uint quantity = uint.Parse(txtQuantity.Text);

            if (hexId.CompareTo(MIN_WEAPON_ID) < 0 || hexId.CompareTo(MAX_WEAPON_ID) > 0)
            {
                selectedInfusion = "Normal";
                selectedUpgrade = "+0";
            }

            _itemSpawnService.SpawnItem(
                baseItemId: formattedId,
                infusionType: selectedInfusion,
                upgradeLevel: selectedUpgrade,
                quantity: quantity,
                durability: 100
            );
        }
    }
}
