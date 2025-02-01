using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS3Tool.services
{
    internal class ItemSpawnService
    {
        private const int itemSpawnOff = 0x7bba70;
        private const int mapItemManOff = 0x4752300;

        Dictionary<string, uint> INFUSION_TYPES = new Dictionary<string, uint>
    {
        { "Normal", 0 },
        { "Heavy", 100 },
        { "Sharp", 200 },
        { "Refined", 300 },
        { "Simple", 400 },
        { "Crystal", 500 },
        { "Fire", 600 },
        { "Chaos", 700 },
        { "Lightning", 800 },
        { "Deep", 900 },
        { "Dark", 1000 },
        { "Poison", 1100 },
        { "Blood", 1200 },
        { "Raw", 1300 },
        { "Blessed", 1400 },
        { "Hollow", 1500 }
    };

        Dictionary<string, uint> UPGRADES = new Dictionary<string, uint>
    {
        { "+0", 0 },
        { "+1", 1 },
        { "+2", 2 },
        { "+3", 3 },
        { "+4", 4 },
        { "+5", 5 },
        { "+6", 6 },
        { "+7", 7 },
        { "+8", 8 },
        { "+9", 9 },
        { "+10", 10 }
    };

        private DS3Process process;

        public ItemSpawnService(DS3Process process)
        {
            this.process = process;
        }

        public struct SpawnRequest
        {
            public uint Unknown { get; set; }
            public uint ItemId { get; set; }
            public uint Qty { get; set; }
            public uint Durability { get; set; }

            public SpawnRequest(uint itemId, uint qty, uint durability)
            {
                Unknown = 1;
                ItemId = itemId;
                Qty = qty;
                Durability = durability;
            }
        }
    }
}
