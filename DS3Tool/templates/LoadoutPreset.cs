using System.Collections.Generic;

namespace DS3Tool.templates
{
    public static class LoadoutPreset

    {

        public static LoadoutTemplate SL1NoUpgrades = new LoadoutTemplate
        {
            Name = "SL1 No Upgrades",

            Items = new List<ItemTemplate>
            {
                new ItemTemplate { ItemName = "Club"},
                new ItemTemplate { ItemName = "Broadsword"},
                new ItemTemplate { ItemName = "Dagger"},
                new ItemTemplate { ItemName = "Bandit's Knife"},
                new ItemTemplate { ItemName = "Storyteller's Staff"},
                new ItemTemplate { ItemName = "Black Knight Sword"},
                new ItemTemplate { ItemName = "Firelink Greatsword"},
                new ItemTemplate { ItemName = "Pyromancy Flame"},
                new ItemTemplate { ItemName = "Great Combustion"},
                new ItemTemplate { ItemName = "Fire Orb"},
                new ItemTemplate { ItemName = "Profaned Greatsword"},
                new ItemTemplate { ItemName = "Irithyll Straight Sword"},
                new ItemTemplate { ItemName = "Torch"},
                new ItemTemplate { ItemName = "Black Flame"},
                new ItemTemplate { ItemName = "Chloranthy Ring"},
                new ItemTemplate { ItemName = "Scholar Ring"},
                new ItemTemplate { ItemName = "Priestess Ring"},
                new ItemTemplate { ItemName = "Red Tearstone Ring"},
                new ItemTemplate { ItemName = "Witch's Ring"},
                new ItemTemplate { ItemName = "Hunter's Ring"},
                new ItemTemplate { ItemName = "Knight's Ring"},
                new ItemTemplate { ItemName = "Carthus Milkring"},
                new ItemTemplate { ItemName = "Fire Clutch Ring"},
                new ItemTemplate { ItemName = "Flynn's Ring"},
                new ItemTemplate { ItemName = "Prisoner's Chain"},
                new ItemTemplate { ItemName = "Carthus Bloodring"},
                new ItemTemplate { ItemName = "Lloyd's Sword Ring"},
                new ItemTemplate { ItemName = "Pontiff's Right Eye"},
                new ItemTemplate { ItemName = "Slumbering Dragoncrest Ring"},

            }
        };

        public static LoadoutTemplate SL1 = new LoadoutTemplate
        {
            Name = "SL1",

            Items = new List<ItemTemplate>
            {
                new ItemTemplate { ItemName = "Club", Infusion="Raw", Upgrade = "+10" },
                new ItemTemplate { ItemName = "Broadsword", Infusion="Raw", Upgrade = "+10" },
                new ItemTemplate { ItemName = "Dagger", Infusion="Raw", Upgrade = "+10" },
                new ItemTemplate { ItemName = "Bandit's Knife", Infusion="Raw", Upgrade = "+10" },
                new ItemTemplate { ItemName = "Storyteller's Staff"},
                new ItemTemplate { ItemName = "Chloranthy Ring"},
                new ItemTemplate { ItemName = "Scholar Ring"},
                new ItemTemplate { ItemName = "Priestess Ring"},
                new ItemTemplate { ItemName = "Red Tearstone Ring"},
                new ItemTemplate { ItemName = "Hunter's Ring"},
                new ItemTemplate { ItemName = "Knight's Ring"},
                new ItemTemplate { ItemName = "Carthus Milkring"},
                new ItemTemplate { ItemName = "Flynn's Ring"},
                new ItemTemplate { ItemName = "Prisoner's Chain"},
                new ItemTemplate { ItemName = "Carthus Bloodring"},
                new ItemTemplate { ItemName = "Lloyd's Sword Ring"},
                new ItemTemplate { ItemName = "Pontiff's Right Eye"},
                new ItemTemplate { ItemName = "Slumbering Dragoncrest Ring"},

            }
        };

        public static LoadoutTemplate MetaLeveled = new LoadoutTemplate
        {
            Name = "Meta",

            Items = new List<ItemTemplate>
            {
                new ItemTemplate { ItemName = "Sellsword Twinblades", Infusion="Sharp", Upgrade = "+10" },
                new ItemTemplate { ItemName = "Sellsword Twinblades"},
                new ItemTemplate { ItemName = "Dagger"},
                new ItemTemplate { ItemName = "Storyteller's Staff"},
                new ItemTemplate { ItemName = "Chloranthy Ring"},
                new ItemTemplate { ItemName = "Red Tearstone Ring"},
                new ItemTemplate { ItemName = "Carthus Milkring"},
                new ItemTemplate { ItemName = "Flynn's Ring"},
                new ItemTemplate { ItemName = "Prisoner's Chain"},
                new ItemTemplate { ItemName = "Carthus Bloodring"},
                new ItemTemplate { ItemName = "Lloyd's Sword Ring"},
                new ItemTemplate { ItemName = "Pontiff's Right Eye"},
                new ItemTemplate { ItemName = "Slumbering Dragoncrest Ring"},

            }
        };
    }
}
