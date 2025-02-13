using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        public Dictionary<string, BonfireLocation> BonfireLocations;
        public BonfireService(DS3Process _ds3Process) {

            _process = _ds3Process;
            readBonfiresFromCSV();
            codeCaveOffset = _process.ds3Base + 0x270BC20;
        }

       

        public struct BonfireLocation
        {
            public int Offset { get; }
            public int StartBit { get; }
            public int Id { get; }

            public BonfireLocation(int offset, int startBit, int Id)
            {
                Offset = offset;
                StartBit = startBit;
                this.Id = Id;
            }
        }


        private void readBonfiresFromCSV()
        {
            BonfireLocations = new Dictionary<string, BonfireLocation>();

            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string path = Path.Combine(projectDirectory, "data", "bonfires.csv");
            string[] lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');
                if (columns.Length >= 4)
                {
                    string name = columns[0].Trim('"', ' ');
                    int offset = Convert.ToInt32(columns[1].Trim('"', ' '), 16);

                    int startBit = int.Parse(columns[2].Trim('"', ' '));
                    int id = int.Parse(columns[3].Trim('"', ' '));

                    BonfireLocation bonfireLocation = new BonfireLocation(offset, startBit, id);

                    BonfireLocations[name] = bonfireLocation;
                }
            }
        }


        public void warpToBonfire(string selectedBonfire)
        {
            InitCodeCave(BonfireLocations[selectedBonfire].Id);
            _process.ReadTestFull(codeBlockStart);
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

            int idRelativeOffset = (int) (bonfireIdOffset.ToInt64() - (codeBlockStart.ToInt64() + codeCave.Length + 4));
            
            codeCave = codeCave.Concat(BitConverter.GetBytes(idRelativeOffset))
                .Concat(new byte[]
                {
                    0x45, 0x8D, 0x80, 0x18, 0xFC, 0xFF, 0xFF,
                    0x45, 0x31, 0xC9,
                    0x48, 0x8B, 0x15,

                }).ToArray();

            int bonfireStateOffset = (int) (_process.ds3Base.ToInt64() + GameManOff + 0xACC - (codeBlockStart.ToInt64() + codeCave.Length + 4));

            codeCave = codeCave.Concat(BitConverter.GetBytes(bonfireStateOffset))
                .Concat(new byte[]
                {
                    0x8D, 0x92, 0x18, 0xFC, 0xFF, 0xFF,
                    0xE8,
                }).ToArray();

            int warpOffset = (int) (_process.ds3Base.ToInt64() + 0x475DC0 - (codeBlockStart.ToInt64() + codeCave.Length + 4));

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
            foreach (var bonfire in BonfireLocations.Keys)
            {
                var ptr1 = _process.ReadUInt64(_process.ds3Base + GameFlagDataOff);
                var ptr2 = _process.ReadUInt64((IntPtr)ptr1);

                var flagLoc = BonfireLocations[bonfire];

                var finalAddress = (IntPtr)(ptr2 + (ulong)flagLoc.Offset);

                uint currentValue = _process.ReadUInt32(finalAddress);
                uint mask = ((1u << 1) - 1) << flagLoc.StartBit;
                currentValue |= mask;

                _process.WriteUInt32(finalAddress, currentValue);
            }
        }

       
    }
}
