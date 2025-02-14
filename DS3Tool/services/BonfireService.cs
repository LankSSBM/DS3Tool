using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static DS3Tool.MainWindow;

namespace DS3Tool.services
{
    internal class BonfireService
    {

        private IntPtr codeCaveOffset;
        private IntPtr codeBlockStart;
        private const int GameFlagDataOff = 0x473BE28;
        private const int GameManOff = 0x4743AB0;
        private const int SprjLuaEventManOff = 0x473A9C8;
        private readonly DS3Process _process;
        public Dictionary<string, BonfireLocation> _bonfireDict;
        public BonfireService(DS3Process _ds3Process, Dictionary<string, BonfireLocation> bonfireDict)
        {

            _process = _ds3Process;
            _bonfireDict = bonfireDict;
            codeCaveOffset = _process.ds3Base + 0x270BC20;
        }

        public void warpToBonfire(string selectedBonfire)
        {
            InitCodeCave(_bonfireDict[selectedBonfire].Id);

            _process.RunThread(codeBlockStart);
            Thread.Sleep(100);
            CleanUpCodeCave();

        }

        private void InitCodeCave(int id)
        {

            IntPtr bonfireIdOffset = codeCaveOffset + 0x20;
            _process.WriteInt32(bonfireIdOffset, id);

            codeBlockStart = codeCaveOffset + 0x40;

            byte[] codeCave = new byte[]
            {
                 0x48, 0x83, 0xEC, 0x48,          //- sub rsp, 48h        ; Allocate stack space
                 0x48, 0x8B, 0x0D,
            };

            int eventOffset = (int)(_process.ds3Base.ToInt64() + SprjLuaEventManOff - (codeBlockStart.ToInt64() + codeCave.Length + 4));


            codeCave = codeCave.Concat(BitConverter.GetBytes(eventOffset))
                .Concat(new byte[]
                {
                  0x4C, 0x8B, 0x05,
            }).ToArray();

            int idRelativeOffset = (int)(bonfireIdOffset.ToInt64() - (codeBlockStart.ToInt64() + codeCave.Length + 4));

            codeCave = codeCave.Concat(BitConverter.GetBytes(idRelativeOffset))
                .Concat(new byte[]
                {
                    0x48, 0x8B, 0x15,
                }).ToArray();

            int bonfireStateOffset = (int)(_process.ds3Base.ToInt64() + GameManOff + 0xACC - (codeBlockStart.ToInt64() + codeCave.Length + 4));

            codeCave = codeCave.Concat(BitConverter.GetBytes(bonfireStateOffset))
                .Concat(new byte[]
                {
                    0x8D, 0x92, 0x18, 0xFC, 0xFF, 0xFF,
                    0xE8,
                }).ToArray();

            int warpOffset = (int)(_process.ds3Base.ToInt64() + 0x475DC0 - (codeBlockStart.ToInt64() + codeCave.Length + 4));

            codeCave = codeCave.Concat(BitConverter.GetBytes(warpOffset))
                .Concat(new byte[]
                {
                    0x48, 0x83, 0xC4, 0x48,
                    0xC3
                }).ToArray();

            _process.WriteBytes(codeBlockStart, codeCave);

        }

        private void CleanUpCodeCave()
        {
            var zeroBytes = new byte[300];
            _process.WriteBytes(codeCaveOffset, zeroBytes);

        }

        public void unlockAllBonfires()
        {
            foreach (var bonfire in _bonfireDict.Keys)
            {
                var ptr1 = _process.ReadUInt64(_process.ds3Base + GameFlagDataOff);
                var ptr2 = _process.ReadUInt64((IntPtr)ptr1);

                var flagLoc = _bonfireDict[bonfire];

                var finalAddress = (IntPtr)(ptr2 + (ulong)flagLoc.Offset);

                uint currentValue = _process.ReadUInt32(finalAddress);
                uint mask = ((1u << 1) - 1) << flagLoc.StartBit;
                currentValue |= mask;

                _process.WriteUInt32(finalAddress, currentValue);
            }
        }


    }
}
