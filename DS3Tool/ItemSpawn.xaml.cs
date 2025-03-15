using DS3Tool.services;
using DS3Tool.templates;
using System.Collections.Generic;
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
        ItemSpawnService _itemSpawnService;
        Dictionary<string, Item> _itemDict;
        List<LoadoutTemplate> Templates;

        public ItemSpawn(DS3Process process, Dictionary<string, Item> itemDict)
        {
            _itemSpawnService = new ItemSpawnService(process);
            _itemDict = itemDict;
            InitializeComponent();
            populate();
            //updateMatch();
        }
        void populate()
        {
            infusionTypeComboBox.Items.Clear();
            infusionTypeComboBox.ItemsSource = _itemSpawnService.InfusionTypes.Keys;
            infusionTypeComboBox.SelectedIndex = 0;

            upgradeComboBox.Items.Clear();
            upgradeComboBox.ItemsSource = _itemSpawnService.Upgrades.Keys;
            upgradeComboBox.SelectedIndex = 0;

            itemList.Items.Clear();
            VirtualizingPanel.SetIsVirtualizing(itemList, true);
            VirtualizingPanel.SetVirtualizationMode(itemList, VirtualizationMode.Recycling);

            Templates = new List<LoadoutTemplate>
            {
                LoadoutPreset.MetaLeveled,
                LoadoutPreset.SL1NoUpgrades,
                LoadoutPreset.SL1
            };
            TemplateComboBox.ItemsSource = Templates;
            TemplateComboBox.SelectedIndex = 0;


            foreach (KeyValuePair<string, Item> item in _itemDict)
            {
                itemList.Items.Add(item.Key);
            }
        }

        private void spawnItem(object sender, RoutedEventArgs e)
        {
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

        private void ApplyTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            if (TemplateComboBox.SelectedItem is LoadoutTemplate selectedTemplate)
            {
                foreach (var item in selectedTemplate.Items)
                {
                    if (_itemDict.TryGetValue(item.ItemName, out Item itemToSpawn))
                    {

                        uint formattedId = uint.Parse(itemToSpawn.Address, System.Globalization.NumberStyles.HexNumber);
                        _itemSpawnService.SpawnItem(
                            baseItemId: formattedId,
                            infusionType: item.Infusion,
                            upgradeLevel: item.Upgrade,
                            quantity: item.Quantity,
                            durability: 100
                        );
                    }
                }
            }
        }
    }
}
