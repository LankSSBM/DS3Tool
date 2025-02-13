using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static DS3Tool.MainWindow;

namespace DS3Tool.services
{
    internal class BonfireService
    {

        private const int GameFlagDataOff = 0x473BE28;
        private readonly DS3Process _process;
        public Dictionary<string, BonfireLocation> _bonfireDict;
        public BonfireService(DS3Process _ds3Process, Dictionary<string, BonfireLocation> bonfireDict) {
            _process = _ds3Process;
            _bonfireDict = bonfireDict;
        }
    
        public void unlockBonfire(string name)
        {

            var ptr1 = _process.ReadUInt64(_process.ds3Base + GameFlagDataOff);
            var ptr2 = _process.ReadUInt64((IntPtr)ptr1);

            var flagLoc = _bonfireDict[name];

            var finalAddress = (IntPtr)(ptr2 + (ulong)flagLoc.Offset);

            uint currentValue = _process.ReadUInt32(finalAddress);
            uint mask = ((1u << 1) - 1) << flagLoc.StartBit;
            currentValue |= mask;

            _process.WriteUInt32(finalAddress, currentValue);
        }

        public void unlockAllBonfires()
        {
            foreach (var bonfire in _bonfireDict.Keys)
            {
                unlockBonfire(bonfire);
            }
        }
    }
}
