using DS3Tool.services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static DS3Tool.MainWindow;

namespace DS3Tool
{
    /// <summary>
    /// Interaction logic for ItemSpawn.xaml
    /// </summary>
    public partial class ItemSpawn : Window
    {
        DS3Process _process;
        ItemSpawnService _itemSpawnService;
        Dictionary<string, Item> _itemDict;

        public ItemSpawn(DS3Process process, Dictionary<string, Item> itemDict)
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

            foreach (KeyValuePair<string, Item> item in _itemDict)
            {
                itemList.Items.Add(item.Key);
            }
        }

        private void spawnItem(object sender, RoutedEventArgs e) {
            const string MIN_WEAPON_ID = "000D9490";
            const string MAX_WEAPON_ID = "015F1AD0";

            if (itemList.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to spawn.");
                return;
            }

            string selectedItem = itemList.SelectedItem.ToString();
            if (!_itemDict.TryGetValue(selectedItem, out Item item))
            {
                MessageBox.Show("Item not found in dictionary.");
                return;
            }

            uint formattedId = uint.Parse(item.Address, System.Globalization.NumberStyles.HexNumber);
            string selectedInfusion = infusionTypeComboBox.SelectedItem.ToString();
            string selectedUpgrade = upgradeComboBox.SelectedItem.ToString();
            uint quantity = uint.Parse(txtQuantity.Text);

            if (item.Address.CompareTo(MIN_WEAPON_ID) < 0 || item.Address.CompareTo(MAX_WEAPON_ID) > 0)
            {
                selectedInfusion = "Normal";
                selectedUpgrade = "+0";
            }
            else if (item.Address.Equals("00A87500"))
            {
                selectedInfusion = "Normal";
                selectedUpgrade = "+0";
            }

            if (string.IsNullOrEmpty(item.Type))
            {
                item.Type = "Default";
            }

            bool isSpecialWeapon = item.Type.Equals("Titanite Scale") || item.Type.Equals("Twinkling Titanite");

            if (isSpecialWeapon && int.Parse(selectedUpgrade.TrimStart('+')) > 5)
            {
                selectedUpgrade = "+5";
            }

            if (isSpecialWeapon)
            {
                selectedInfusion = "Normal";
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
