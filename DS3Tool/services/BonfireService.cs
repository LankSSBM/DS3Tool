using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS3Tool.services
{
    internal class BonfireService
    {

        private const int GameFlagDataOff = 0x473BE28;
        private readonly DS3Process _process;
        public BonfireService(DS3Process _ds3Process) {

            _process = _ds3Process;
        }

        public struct BonfireLocation
        {
            public int Offset { get; }
            public int StartBit { get; }

            public BonfireLocation(int offset, int startBit)
            {
                Offset = offset;
                StartBit = startBit;
            }
        }

        public readonly Dictionary<string, BonfireLocation> BonfireLocations = new Dictionary<string, BonfireLocation>
        {
            {"Coiled Sword", new BonfireLocation(0x5A0F, startBit: 2) },
            {"Cemetery of Ash", new BonfireLocation(0x5A03, startBit: 6)},
            {"Iudex Gundyr", new BonfireLocation(0x5A03, startBit: 5)},
            {"Firelink Shrine", new BonfireLocation(0x5A03, startBit: 7)},
            {"Untended Graves", new BonfireLocation(0x5A03, startBit: 4)},
            {"Champion Gundyr", new BonfireLocation(0x5A03, startBit: 3)},
            {"High Wall of Lothric", new BonfireLocation(0xF02, startBit: 6)},
            {"Tower on the Wall", new BonfireLocation(0xF03, startBit: 2)},
            {"Vordt of the Boreal Valley", new BonfireLocation(0xF03, startBit: 5)},
            {"Dancer of the Boreal Valley", new BonfireLocation(0xF03, startBit: 3)},
            {"Oceiros the Consumed King", new BonfireLocation(0xF03, startBit: 6)},
            {"Foot of the High Wall", new BonfireLocation(0x1903, startBit: 3)},
            {"Undead Settlement", new BonfireLocation(0x1903, startBit: 7)},
            {"Dilapidated Bridge", new BonfireLocation(0x1903, startBit: 4)},
            {"Cliff Underside", new BonfireLocation(0x1903, startBit: 5)},
            {"Pit of Hollows", new BonfireLocation(0x2D03, startBit: 1)},
            {"Road of Sacrifices", new BonfireLocation(0x2D03, startBit: 1)},
            {"Halfway Fortress", new BonfireLocation(0x2D03, startBit: 7)},
            {"Crucifixion Woods", new BonfireLocation(0x2D03, startBit: 0)},
            {"Farron Keep", new BonfireLocation(0x2D03, startBit: 4)},
            {"Keep Ruins", new BonfireLocation(0x2D03, startBit: 3)},
            {"Old Wolf of Farron", new BonfireLocation(0x2D03, startBit: 2)},
            {"Cathedral of the Deep", new BonfireLocation(0x3C03, startBit: 4)},
            {"Cleansing Chapel", new BonfireLocation(0x3C03, startBit: 7)},
            {"Rosaria's Bed Chamber", new BonfireLocation(0x3C03, startBit: 5)},
            {"Catacombs of Carthus", new BonfireLocation(0x5003, startBit: 1)},
            {"Abandoned Tomb", new BonfireLocation(0x5003, startBit: 6)},
            {"Demon Ruins", new BonfireLocation(0x5003, startBit: 4)},
            {"Old King's Antechamber", new BonfireLocation(0x5003, startBit: 5)},
            {"Irithyll of the Boreal Valley", new BonfireLocation(0x4B03, startBit: 0)},
            {"Central Irithyll", new BonfireLocation(0x4B03, startBit: 3)},
            {"Church of Yorshka", new BonfireLocation(0x4B03, startBit: 7)},
            {"Distant Manor", new BonfireLocation(0x4B03, startBit: 2)},
            {"Water Reserve", new BonfireLocation(0x4B03, startBit: 1)},
            {"Anor Londo", new BonfireLocation(0x4B03, startBit: 4)},
            {"Prison Tower", new BonfireLocation(0x4B02, startBit: 7)},
            {"Irithyll Dungeon", new BonfireLocation(0x5503, startBit: 7)},
            {"Profaned Capital", new BonfireLocation(0x5503, startBit: 5)},
            {"Archdragon Peak", new BonfireLocation(0x2303, startBit: 7)},
            {"Dragon-Kin Mausoleum", new BonfireLocation(0x2303, startBit: 4)},
            {"Great Belfry", new BonfireLocation(0x2303, startBit: 5)},
            {"Lothric Castle", new BonfireLocation(0x1403, startBit: 7)},
            {"Dragon Barracks", new BonfireLocation(0x1403, startBit: 5)},
            {"Grand Archives", new BonfireLocation(0x3703, startBit: 6)},
            {"Flameless Shrine", new BonfireLocation(0x5F03, startBit: 7)},
            {"Kiln of the First Flame", new BonfireLocation(0x5F03, startBit: 6)},
            {"Snowfield", new BonfireLocation(0x6403, startBit: 6)},
            {"Rope Bridge Cave", new BonfireLocation(0x6403, startBit: 5)},
            {"Ariandel Chapel", new BonfireLocation(0x6403, startBit: 2)},
            {"Corvian Settlement", new BonfireLocation(0x6403, startBit: 4)},
            {"Snowy Mountain Pass", new BonfireLocation(0x6403, startBit: 3)},
            {"Depths of the Painting", new BonfireLocation(0x6403, startBit: 0)},
            {"The Dreg Heap", new BonfireLocation(0x7303, startBit: 6)},
            {"Earthen Peak Ruins", new BonfireLocation(0x7303, startBit: 5)},
            {"Within the Earthen Peak Ruins", new BonfireLocation(0x7303, startBit: 4)},
            {"The Demon Prince", new BonfireLocation(0x7303, startBit: 7)},
            {"Mausoleum Lookout", new BonfireLocation(0x7803, startBit: 5)},
            {"Ringed Inner Wall", new BonfireLocation(0x7803, startBit: 4)},
            {"Ringed City Streets", new BonfireLocation(0x7803, startBit: 3)},
            {"Shared Grave", new BonfireLocation(0x7803, startBit: 2)},
            {"Church of Filianore", new BonfireLocation(0x7803, startBit: 7)},
            {"Darkeater Midir", new BonfireLocation(0x7803, startBit: 6)},
            {"Filianore's Rest", new BonfireLocation(0x7D03, startBit: 6)},
            {"Slave Knight Gael", new BonfireLocation(0x7D03, startBit: 7)},
        };
    
        public void unlockBonfire(string name)
        {

            var ptr1 = _process.ReadUInt64(_process.ds3Base + GameFlagDataOff);
            var ptr2 = _process.ReadUInt64((IntPtr)ptr1);

            var flagLoc = BonfireLocations[name];

            var finalAddress = (IntPtr)(ptr2 + (ulong)flagLoc.Offset);

            uint currentValue = _process.ReadUInt32(finalAddress);
            uint mask = ((1u << 1) - 1) << flagLoc.StartBit;
            currentValue |= mask;

            _process.WriteUInt32(finalAddress, currentValue);
        }

        public void unlockAllBonfires()
        {
            foreach (var bonfire in BonfireLocations.Keys)
            {
                unlockBonfire(bonfire);
            }
        }
    }
}
