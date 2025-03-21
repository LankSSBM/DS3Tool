﻿using System.Collections.Generic;

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

        public static LoadoutTemplate AllRings = new LoadoutTemplate
        {
            Name = "All Rings",

            Items = new List<ItemTemplate> {
                new ItemTemplate { ItemName = "Ashen Estus Ring"},
                new ItemTemplate { ItemName = "Bellowing Dragoncrest Ring"},
                new ItemTemplate { ItemName = "Bloodbite Ring"},
                new ItemTemplate { ItemName = "Bloodbite Ring+1"},
                new ItemTemplate { ItemName = "Blue Tearstone Ring"},
                new ItemTemplate { ItemName = "Calamity Ring"},
                new ItemTemplate { ItemName = "Carthus Bloodring"},
                new ItemTemplate { ItemName = "Carthus Milkring"},
                new ItemTemplate { ItemName = "Chillbite Ring"},
                new ItemTemplate { ItemName = "Chloranthy Ring"},
                new ItemTemplate { ItemName = "Chloranthy Ring+1"},
                new ItemTemplate { ItemName = "Chloranthy Ring+2"},
                new ItemTemplate { ItemName = "Chloranthy Ring+3"},
                new ItemTemplate { ItemName = "Covetous Gold Serpent Ring"},
                new ItemTemplate { ItemName = "Covetous Gold Serpent Ring+1"},
                new ItemTemplate { ItemName = "Covetous Gold Serpent Ring+2"},
                new ItemTemplate { ItemName = "Covetous Gold Serpent Ring+3"},
                new ItemTemplate { ItemName = "Covetous Silver Serpent Ring"},
                new ItemTemplate { ItemName = "Covetous Silver Serpent Ring+1"},
                new ItemTemplate { ItemName = "Covetous Silver Serpent Ring+2"},
                new ItemTemplate { ItemName = "Covetous Silver Serpent Ring+3"},
                new ItemTemplate { ItemName = "Cursebite Ring"},
                new ItemTemplate { ItemName = "Dark Clutch Ring"},
                new ItemTemplate { ItemName = "Dark Stoneplate Ring"},
                new ItemTemplate { ItemName = "Dark Stoneplate Ring+1"},
                new ItemTemplate { ItemName = "Dark Stoneplate Ring+2"},
                new ItemTemplate { ItemName = "Darkmoon Ring"},
                new ItemTemplate { ItemName = "Deep Ring"},
                new ItemTemplate { ItemName = "Dragonscale Ring"},
                new ItemTemplate { ItemName = "Dusk Crown Ring"},
                new ItemTemplate { ItemName = "Estus Ring"},
                new ItemTemplate { ItemName = "Farron Ring"},
                new ItemTemplate { ItemName = "Fire Clutch Ring"},
                new ItemTemplate { ItemName = "Flame Stoneplate Ring"},
                new ItemTemplate { ItemName = "Flame Stoneplate Ring+1"},
                new ItemTemplate { ItemName = "Flame Stoneplate Ring+2"},
                new ItemTemplate { ItemName = "Fleshbite Ring"},
                new ItemTemplate { ItemName = "Fleshbite Ring+1"},
                new ItemTemplate { ItemName = "Flynn's Ring"},
                new ItemTemplate { ItemName = "Great Swamp Ring"},
                new ItemTemplate { ItemName = "Havel's Ring"},
                new ItemTemplate { ItemName = "Havel's Ring+1"},
                new ItemTemplate { ItemName = "Havel's Ring+2"},
                new ItemTemplate { ItemName = "Havel's Ring+3"},
                new ItemTemplate { ItemName = "Hawk Ring"},
                new ItemTemplate { ItemName = "Hornet Ring"},
                new ItemTemplate { ItemName = "Horsehoof Ring"},
                new ItemTemplate { ItemName = "Hunter's Ring"},
                new ItemTemplate { ItemName = "Jailer's Key Ring"},
                new ItemTemplate { ItemName = "Knight Slayer's Ring"},
                new ItemTemplate { ItemName = "Knight's Ring"},
                new ItemTemplate { ItemName = "Leo Ring"},
                new ItemTemplate { ItemName = "Life Ring"},
                new ItemTemplate { ItemName = "Life Ring+1"},
                new ItemTemplate { ItemName = "Life Ring+2"},
                new ItemTemplate { ItemName = "Life Ring+3"},
                new ItemTemplate { ItemName = "Lightning Clutch Ring"},
                new ItemTemplate { ItemName = "Lingering Dragoncrest Ring"},
                new ItemTemplate { ItemName = "Lingering Dragoncrest Ring+1"},
                new ItemTemplate { ItemName = "Lingering Dragoncrest Ring+2"},
                new ItemTemplate { ItemName = "Lloyd's Shield Ring"},
                new ItemTemplate { ItemName = "Lloyd's Sword Ring"},
                new ItemTemplate { ItemName = "Magic Clutch Ring"},
                new ItemTemplate { ItemName = "Magic Stoneplate Ring"},
                new ItemTemplate { ItemName = "Magic Stoneplate Ring+1"},
                new ItemTemplate { ItemName = "Magic Stoneplate Ring+2"},
                new ItemTemplate { ItemName = "Morne's Ring"},
                new ItemTemplate { ItemName = "Obscuring Ring"},
                new ItemTemplate { ItemName = "Poisonbite Ring"},
                new ItemTemplate { ItemName = "Poisonbite Ring+1"},
                new ItemTemplate { ItemName = "Priestess Ring"},
                new ItemTemplate { ItemName = "Red Tearstone Ring"},
                new ItemTemplate { ItemName = "Reversal Ring"},
                new ItemTemplate { ItemName = "Ring of Favor"},
                new ItemTemplate { ItemName = "Ring of Favor+1"},
                new ItemTemplate { ItemName = "Ring of Favor+2"},
                new ItemTemplate { ItemName = "Ring of Favor+3"},
                new ItemTemplate { ItemName = "Ring of Sacrifice"},
                new ItemTemplate { ItemName = "Ring of Steel Protection"},
                new ItemTemplate { ItemName = "Ring of Steel Protection+1"},
                new ItemTemplate { ItemName = "Ring of Steel Protection+2"},
                new ItemTemplate { ItemName = "Ring of Steel Protection+3"},
                new ItemTemplate { ItemName = "Ring of the Evil Eye"},
                new ItemTemplate { ItemName = "Ring of the Evil Eye+1"},
                new ItemTemplate { ItemName = "Ring of the Evil Eye+2"},
                new ItemTemplate { ItemName = "Ring of the Evil Eye+3"},
                new ItemTemplate { ItemName = "Ring of the Sun's First Born"},
                new ItemTemplate { ItemName = "Sage Ring"},
                new ItemTemplate { ItemName = "Sage Ring+1"},
                new ItemTemplate { ItemName = "Sage Ring+2"},
                new ItemTemplate { ItemName = "Saint's Ring"},
                new ItemTemplate { ItemName = "Scholar Ring"},
                new ItemTemplate { ItemName = "Silvercat Ring"},
                new ItemTemplate { ItemName = "Skull Ring"},
                new ItemTemplate { ItemName = "Slumbering Dragoncrest Ring"},
                new ItemTemplate { ItemName = "Speckled Stoneplate Ring"},
                new ItemTemplate { ItemName = "Speckled Stoneplate Ring+1"},
                new ItemTemplate { ItemName = "Sun Princess Ring"},
                new ItemTemplate { ItemName = "Thunder Stoneplate Ring"},
                new ItemTemplate { ItemName = "Thunder Stoneplate Ring+1"},
                new ItemTemplate { ItemName = "Thunder Stoneplate Ring+2"},
                new ItemTemplate { ItemName = "Untrue Dark Ring"},
                new ItemTemplate { ItemName = "Untrue White Ring"},
                new ItemTemplate { ItemName = "Witch's Ring"},
                new ItemTemplate { ItemName = "Wolf Ring"},
                new ItemTemplate { ItemName = "Wolf Ring+1"},
                new ItemTemplate { ItemName = "Wolf Ring+2"},
                new ItemTemplate { ItemName = "Wolf Ring+3"},
                new ItemTemplate { ItemName = "Wood Grain Ring"},
                new ItemTemplate { ItemName = "Wood Grain Ring+1"},
                new ItemTemplate { ItemName = "Wood Grain Ring+2"},
                new ItemTemplate { ItemName = "Young Dragon Ring"}
            }
        };

        public static LoadoutTemplate AllWeapons = new LoadoutTemplate
        {
            Name = "All Weapons",

            Items = new List<ItemTemplate> {
                new ItemTemplate { ItemName = "Ancient Dragon Greatshield" },
                new ItemTemplate { ItemName = "Anri's Straight Sword" },
                new ItemTemplate { ItemName = "Aquamarine Dagger" },
                new ItemTemplate { ItemName = "Arbalest" },
                new ItemTemplate { ItemName = "Archdeacon's Great Staff" },
                new ItemTemplate { ItemName = "Arstor's Spear" },
                new ItemTemplate { ItemName = "Astora Greatsword" },
                new ItemTemplate { ItemName = "Astora Straight Sword" },
                new ItemTemplate { ItemName = "Avelyn" },
                new ItemTemplate { ItemName = "Bandit's Knife" },
                new ItemTemplate { ItemName = "Barbed Straight Sword" },
                new ItemTemplate { ItemName = "Bastard Sword" },
                new ItemTemplate { ItemName = "Battle Axe" },
                new ItemTemplate { ItemName = "Black Blade" },
                new ItemTemplate { ItemName = "Black Bow of Pharis" },
                new ItemTemplate { ItemName = "Black Iron Greatshield" },
                new ItemTemplate { ItemName = "Black Knight Glaive" },
                new ItemTemplate { ItemName = "Black Knight Greataxe" },
                new ItemTemplate { ItemName = "Black Knight Greatsword" },
                new ItemTemplate { ItemName = "Black Knight Shield" },
                new ItemTemplate { ItemName = "Black Knight Sword" },
                new ItemTemplate { ItemName = "Blacksmith Hammer" },
                new ItemTemplate { ItemName = "Bloodlust" },
                new ItemTemplate { ItemName = "Blue Wooden Shield" },
                new ItemTemplate { ItemName = "Bonewheel Shield" },
                new ItemTemplate { ItemName = "Brigand Axe" },
                new ItemTemplate { ItemName = "Brigand Twindaggers" },
                new ItemTemplate { ItemName = "Broadsword" },
                new ItemTemplate { ItemName = "Broken Straight Sword" },
                new ItemTemplate { ItemName = "Buckler" },
                new ItemTemplate { ItemName = "Butcher Knife" },
                new ItemTemplate { ItemName = "Caduceus Round Shield" },
                new ItemTemplate { ItemName = "Caestus" },
                new ItemTemplate { ItemName = "Caitha's Chime" },
                new ItemTemplate { ItemName = "Canvas Talisman" },
                new ItemTemplate { ItemName = "Carthus Curved Greatsword" },
                new ItemTemplate { ItemName = "Carthus Curved Sword" },
                new ItemTemplate { ItemName = "Carthus Shield" },
                new ItemTemplate { ItemName = "Carthus Shotel" },
                new ItemTemplate { ItemName = "Cathedral Knight Greatshield" },
                new ItemTemplate { ItemName = "Cathedral Knight Greatsword" },
                new ItemTemplate { ItemName = "Chaos Blade" },
                new ItemTemplate { ItemName = "Claw" },
                new ItemTemplate { ItemName = "Claymore" },
                new ItemTemplate { ItemName = "Cleric's Candlestick" },
                new ItemTemplate { ItemName = "Cleric's Sacred Chime" },
                new ItemTemplate { ItemName = "Club" },
                new ItemTemplate { ItemName = "Composite Bow" },
                new ItemTemplate { ItemName = "Corvian Greatknife" },
                new ItemTemplate { ItemName = "Court Sorcerer's Staff" },
                new ItemTemplate { ItemName = "Crescent Axe" },
                new ItemTemplate { ItemName = "Crescent Moon Sword" },
                new ItemTemplate { ItemName = "Crest Shield" },
                new ItemTemplate { ItemName = "Crimson Parma" },
                new ItemTemplate { ItemName = "Crow Quills" },
                new ItemTemplate { ItemName = "Crow Talons" },
                new ItemTemplate { ItemName = "Crucifix of the Mad King" },
                new ItemTemplate { ItemName = "Crystal Chime" },
                new ItemTemplate { ItemName = "Crystal Sage's Rapier" },
                new ItemTemplate { ItemName = "Curse Ward Greatshield" },
                new ItemTemplate { ItemName = "Dagger" },
                new ItemTemplate { ItemName = "Dancer's Enchanted Swords" },
                new ItemTemplate { ItemName = "Dark Hand" },
                new ItemTemplate { ItemName = "Dark Sword" },
                new ItemTemplate { ItemName = "Darkdrift" },
                new ItemTemplate { ItemName = "Darkmoon Longbow" },
                new ItemTemplate { ItemName = "Demon's Fist" },
                new ItemTemplate { ItemName = "Demon's Greataxe" },
                new ItemTemplate { ItemName = "Demon's Scar" },
                new ItemTemplate { ItemName = "Dragon Crest Shield" },
                new ItemTemplate { ItemName = "Dragon Tooth" },
                new ItemTemplate { ItemName = "Dragonhead Greatshield" },
                new ItemTemplate { ItemName = "Dragonhead Shield" },
                new ItemTemplate { ItemName = "Dragonrider Bow" },
                new ItemTemplate { ItemName = "Dragonslayer Greataxe" },
                new ItemTemplate { ItemName = "Dragonslayer Greatbow" },
                new ItemTemplate { ItemName = "Dragonslayer Greatshield" },
                new ItemTemplate { ItemName = "Dragonslayer Spear" },
                new ItemTemplate { ItemName = "Dragonslayer Swordspear" },
                new ItemTemplate { ItemName = "Dragonslayer's Axe" },
                new ItemTemplate { ItemName = "Drakeblood Greatsword" },
                new ItemTemplate { ItemName = "Drang Hammers" },
                new ItemTemplate { ItemName = "Drang Twinspears" },
                new ItemTemplate { ItemName = "Earth Seeker" },
                new ItemTemplate { ItemName = "East-West Shield" },
                new ItemTemplate { ItemName = "Eastern Iron Shield" },
                new ItemTemplate { ItemName = "Eleonora" },
                new ItemTemplate { ItemName = "Elkhorn Round Shield" },
                new ItemTemplate { ItemName = "Estoc" },
                new ItemTemplate { ItemName = "Ethereal Oak Shield" },
                new ItemTemplate { ItemName = "Executioner's Greatsword" },
                new ItemTemplate { ItemName = "Exile Greatsword" },
                new ItemTemplate { ItemName = "Falchion" },
                new ItemTemplate { ItemName = "Farron Greatsword" },
                new ItemTemplate { ItemName = "Firelink Greatsword" },
                new ItemTemplate { ItemName = "Fist" },
                new ItemTemplate { ItemName = "Flamberge" },
                new ItemTemplate { ItemName = "Follower Javelin" },
                new ItemTemplate { ItemName = "Follower Sabre" },
                new ItemTemplate { ItemName = "Follower Shield" },
                new ItemTemplate { ItemName = "Follower Torch" },
                new ItemTemplate { ItemName = "Four-Pronged Plow" },
                new ItemTemplate { ItemName = "Frayed Blade" },
                new ItemTemplate { ItemName = "Friede's Great Scythe" },
                new ItemTemplate { ItemName = "Fume Ultra Greatsword" },
                new ItemTemplate { ItemName = "Gael's Greatsword" },
                new ItemTemplate { ItemName = "Gargoyle Flame Hammer" },
                new ItemTemplate { ItemName = "Gargoyle Flame Spear" },
                new ItemTemplate { ItemName = "Ghru Rotshield" },
                new ItemTemplate { ItemName = "Giant Door Shield" },
                new ItemTemplate { ItemName = "Glaive" },
                new ItemTemplate { ItemName = "Golden Falcon Shield" },
                new ItemTemplate { ItemName = "Golden Ritual Spear" },
                new ItemTemplate { ItemName = "Golden Wing Crest Shield" },
                new ItemTemplate { ItemName = "Gotthard Twinswords" },
                new ItemTemplate { ItemName = "Grass Crest Shield" },
                new ItemTemplate { ItemName = "Great Club" },
                new ItemTemplate { ItemName = "Great Corvian Scythe" },
                new ItemTemplate { ItemName = "Great Mace" },
                new ItemTemplate { ItemName = "Great Machete" },
                new ItemTemplate { ItemName = "Great Scythe" },
                new ItemTemplate { ItemName = "Great Wooden Hammer" },
                new ItemTemplate { ItemName = "Greataxe" },
                new ItemTemplate { ItemName = "Greatlance" },
                new ItemTemplate { ItemName = "Greatshield of Glory" },
                new ItemTemplate { ItemName = "Greatsword" },
                new ItemTemplate { ItemName = "Greatsword of Judgment" },
                new ItemTemplate { ItemName = "Gundyr's Halberd" },
                new ItemTemplate { ItemName = "Halberd" },
                new ItemTemplate { ItemName = "Hand Axe" },
                new ItemTemplate { ItemName = "Handmaid's Dagger" },
                new ItemTemplate { ItemName = "Harald Curved Greatsword" },
                new ItemTemplate { ItemName = "Harpe" },
                new ItemTemplate { ItemName = "Havel's Greatshield" },
                new ItemTemplate { ItemName = "Hawkwood's Shield" },
                new ItemTemplate { ItemName = "Heavy Crossbow" },
                new ItemTemplate { ItemName = "Heretic's Staff" },
                new ItemTemplate { ItemName = "Heysel Pick" },
                new ItemTemplate { ItemName = "Hollowslayer Greatsword" },
                new ItemTemplate { ItemName = "Immolation Tinder" },
                new ItemTemplate { ItemName = "Irithyll Rapier" },
                new ItemTemplate { ItemName = "Irithyll Straight Sword" },
                new ItemTemplate { ItemName = "Iron Round Shield" },
                new ItemTemplate { ItemName = "Izalith Staff" },
                new ItemTemplate { ItemName = "Kite Shield" },
                new ItemTemplate { ItemName = "Knight Shield" },
                new ItemTemplate { ItemName = "Knight's Crossbow" },
                new ItemTemplate { ItemName = "Large Club" },
                new ItemTemplate { ItemName = "Large Leather Shield" },
                new ItemTemplate { ItemName = "Leather Shield" },
                new ItemTemplate { ItemName = "Ledo's Great Hammer" },
                new ItemTemplate { ItemName = "Light Crossbow" },
                new ItemTemplate { ItemName = "Llewellyn Shield" },
                new ItemTemplate { ItemName = "Long Sword" },
                new ItemTemplate { ItemName = "Longbow" },
                new ItemTemplate { ItemName = "Lorian's Greatsword" },
                new ItemTemplate { ItemName = "Lothric Knight Greatshield" },
                new ItemTemplate { ItemName = "Lothric Knight Greatsword" },
                new ItemTemplate { ItemName = "Lothric Knight Long Spear" },
                new ItemTemplate { ItemName = "Lothric Knight Shield" },
                new ItemTemplate { ItemName = "Lothric Knight Sword" },
                new ItemTemplate { ItemName = "Lothric War Banner" },
                new ItemTemplate { ItemName = "Lothric's Holy Sword" },
                new ItemTemplate { ItemName = "Lucerne" },
                new ItemTemplate { ItemName = "Mace" },
                new ItemTemplate { ItemName = "Mail Breaker" },
                new ItemTemplate { ItemName = "Man Serpent Hatchet" },
                new ItemTemplate { ItemName = "Man-grub's Staff" },
                new ItemTemplate { ItemName = "Manikin Claws" },
                new ItemTemplate { ItemName = "Mendicant's Staff" },
                new ItemTemplate { ItemName = "Millwood Battle Axe" },
                new ItemTemplate { ItemName = "Millwood Greatbow" },
                new ItemTemplate { ItemName = "Moaning Shield" },
                new ItemTemplate { ItemName = "Moonlight Greatsword" },
                new ItemTemplate { ItemName = "Morion Blade" },
                new ItemTemplate { ItemName = "Morne's Great Hammer" },
                new ItemTemplate { ItemName = "Morning Star" },
                new ItemTemplate { ItemName = "Murakumo" },
                new ItemTemplate { ItemName = "Murky Hand Scythe" },
                new ItemTemplate { ItemName = "Murky Longstaff" },
                new ItemTemplate { ItemName = "Notched Whip" },
                new ItemTemplate { ItemName = "Old King's Great Hammer" },
                new ItemTemplate { ItemName = "Old Wolf Curved Sword" },
                new ItemTemplate { ItemName = "Onikiri and Ubadachi" },
                new ItemTemplate { ItemName = "Onislayer Greatbow" },
                new ItemTemplate { ItemName = "Onyx Blade" },
                new ItemTemplate { ItemName = "Painting Guardian's Curved Sword" },
                new ItemTemplate { ItemName = "Parrying Dagger" },
                new ItemTemplate { ItemName = "Partizan" },
                new ItemTemplate { ItemName = "Pickaxe" },
                new ItemTemplate { ItemName = "Pierce Shield" },
                new ItemTemplate { ItemName = "Pike" },
                new ItemTemplate { ItemName = "Plank Shield" },
                new ItemTemplate { ItemName = "Pontiff Knight Curved Sword" },
                new ItemTemplate { ItemName = "Pontiff Knight Great Scythe" },
                new ItemTemplate { ItemName = "Pontiff Knight Shield" },
                new ItemTemplate { ItemName = "Porcine Shield" },
                new ItemTemplate { ItemName = "Preacher's Right Arm" },
                new ItemTemplate { ItemName = "Priest's Chime" },
                new ItemTemplate { ItemName = "Profaned Greatsword" },
                new ItemTemplate { ItemName = "Pyromancer's Parting Flame" },
                new ItemTemplate { ItemName = "Pyromancy Flame" },
                new ItemTemplate { ItemName = "Quakestone Hammer" },
                new ItemTemplate { ItemName = "Rapier" },
                new ItemTemplate { ItemName = "Red and White Round Shield" },
                new ItemTemplate { ItemName = "Red Hilted Halberd" },
                new ItemTemplate { ItemName = "Reinforced Club" },
                new ItemTemplate { ItemName = "Repeating Crossbow" },
                new ItemTemplate { ItemName = "Ricard's Rapier" },
                new ItemTemplate { ItemName = "Ringed Knight Paired Greatswords" },
                new ItemTemplate { ItemName = "Ringed Knight Spear" },
                new ItemTemplate { ItemName = "Ringed Knight Straight Sword" },
                new ItemTemplate { ItemName = "Rose of Ariandel" },
                new ItemTemplate { ItemName = "Rotten Ghru Curved Sword" },
                new ItemTemplate { ItemName = "Rotten Ghru Dagger" },
                new ItemTemplate { ItemName = "Rotten Ghru Spear" },
                new ItemTemplate { ItemName = "Round Shield" },
                new ItemTemplate { ItemName = "Sacred Bloom Shield" },
                new ItemTemplate { ItemName = "Sacred Chime of Filianore" },
                new ItemTemplate { ItemName = "Sage's Crystal Staff" },
                new ItemTemplate { ItemName = "Saint Bident" },
                new ItemTemplate { ItemName = "Saint-tree Bellvine" },
                new ItemTemplate { ItemName = "Saint's Talisman" },
                new ItemTemplate { ItemName = "Scholar's Candlestick" },
                new ItemTemplate { ItemName = "Scimitar" },
                new ItemTemplate { ItemName = "Sellsword Twinblades" },
                new ItemTemplate { ItemName = "Shield of Want" },
                new ItemTemplate { ItemName = "Short Bow" },
                new ItemTemplate { ItemName = "Shortsword" },
                new ItemTemplate { ItemName = "Shotel" },
                new ItemTemplate { ItemName = "Silver Eagle Kite Shield" },
                new ItemTemplate { ItemName = "Silver Knight Shield" },
                new ItemTemplate { ItemName = "Small Leather Shield" },
                new ItemTemplate { ItemName = "Smough's Great Hammer" },
                new ItemTemplate { ItemName = "Sniper Crossbow" },
                new ItemTemplate { ItemName = "Soldering Iron" },
                new ItemTemplate { ItemName = "Sorcerer's Staff" },
                new ItemTemplate { ItemName = "Spear" },
                new ItemTemplate { ItemName = "Spider Shield" },
                new ItemTemplate { ItemName = "Spiked Mace" },
                new ItemTemplate { ItemName = "Spiked Shield" },
                new ItemTemplate { ItemName = "Spirit Tree Crest Shield" },
                new ItemTemplate { ItemName = "Splitleaf Greatsword" },
                new ItemTemplate { ItemName = "Spotted Whip" },
                new ItemTemplate { ItemName = "Stone Greatshield" },
                new ItemTemplate { ItemName = "Stone Parma" },
                new ItemTemplate { ItemName = "Storm Curved Sword" },
                new ItemTemplate { ItemName = "Storm Ruler" },
                new ItemTemplate { ItemName = "Storyteller's Staff" },
                new ItemTemplate { ItemName = "Sunless Talisman" },
                new ItemTemplate { ItemName = "Sunlight Shield" },
                new ItemTemplate { ItemName = "Sunlight Straight Sword" },
                new ItemTemplate { ItemName = "Sunlight Talisman" },
                new ItemTemplate { ItemName = "Sunset Shield" },
                new ItemTemplate { ItemName = "Tailbone Short Sword" },
                new ItemTemplate { ItemName = "Tailbone Spear" },
                new ItemTemplate { ItemName = "Talisman" },
                new ItemTemplate { ItemName = "Target Shield" },
                new ItemTemplate { ItemName = "Thrall Axe" },
                new ItemTemplate { ItemName = "Torch" },
                new ItemTemplate { ItemName = "Twin Dragon Greatshield" },
                new ItemTemplate { ItemName = "Twin Princes' Greatsword" },
                new ItemTemplate { ItemName = "Uchigatana" },
                new ItemTemplate { ItemName = "Valorheart" },
                new ItemTemplate { ItemName = "Vordt's Great Hammer" },
                new ItemTemplate { ItemName = "Warden Twinblades" },
                new ItemTemplate { ItemName = "Wargod Wooden Shield" },
                new ItemTemplate { ItemName = "Warpick" },
                new ItemTemplate { ItemName = "Warrior's Round Shield" },
                new ItemTemplate { ItemName = "Washing Pole" },
                new ItemTemplate { ItemName = "Whip" },
                new ItemTemplate { ItemName = "White Birch Bow" },
                new ItemTemplate { ItemName = "White Hair Talisman" },
                new ItemTemplate { ItemName = "Winged Knight Halberd" },
                new ItemTemplate { ItemName = "Winged Knight Twinaxes" },
                new ItemTemplate { ItemName = "Winged Spear" },
                new ItemTemplate { ItemName = "Witch's Locks" },
                new ItemTemplate { ItemName = "Witchtree Branch" },
                new ItemTemplate { ItemName = "Wolf Knight's Greatshield" },
                new ItemTemplate { ItemName = "Wolf Knight's Greatsword" },
                new ItemTemplate { ItemName = "Wolnir's Holy Sword" },
                new ItemTemplate { ItemName = "Wooden Shield" },
                new ItemTemplate { ItemName = "Yhorm's Great Machete" },
                new ItemTemplate { ItemName = "Yhorm's Greatshield" },
                new ItemTemplate { ItemName = "Yorshka's Chime" },
                new ItemTemplate { ItemName = "Yorshka's Spear" },
                new ItemTemplate { ItemName = "Zweihander" }
            }
        };
    }
}
