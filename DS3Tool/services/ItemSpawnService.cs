using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DS3Tool.services
{
    internal class ItemSpawnService
    {
        private const int ItemSpawnOffset = 0x7bba70;
        private const int MapItemManagerOffset = 0x4752300;
        private const long BaseMemoryLocation = 0x143B40C1E;

        public readonly Dictionary<string, uint> InfusionTypes = new Dictionary<string, uint>
        {
            { "Normal", 0 }, { "Heavy", 100 }, { "Sharp", 200 },
            { "Refined", 300 }, { "Simple", 400 }, { "Crystal", 500 },
            { "Fire", 600 }, { "Chaos", 700 }, { "Lightning", 800 },
            { "Deep", 900 }, { "Dark", 1000 }, { "Poison", 1100 },
            { "Blood", 1200 }, { "Raw", 1300 }, { "Blessed", 1400 },
            { "Hollow", 1500 }
        };

        public readonly Dictionary<string, uint> Upgrades = new Dictionary<string, uint>
        {
            { "+0", 0 }, { "+1", 1 }, { "+2", 2 }, { "+3", 3 }, { "+4", 4 },
            { "+5", 5 }, { "+6", 6 }, { "+7", 7 }, { "+8", 8 }, { "+9", 9 },
            { "+10", 10 }
        };

        private readonly DS3Process _process;

        [StructLayout(LayoutKind.Sequential)]
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

        public ItemSpawnService(DS3Process process)
        {
            this._process = process;
        }

        public void SpawnItem(uint baseItemId, string infusionType = "Normal", string upgradeLevel = "+0", uint quantity = 1, uint durability = 0)
        {

            IntPtr requestLocation = new IntPtr(BaseMemoryLocation);
            IntPtr outputLocation = new IntPtr(BaseMemoryLocation + 100);
            IntPtr shellcodeLocation = new IntPtr(BaseMemoryLocation + 200);
            try
            {

                uint finalItemId = baseItemId + InfusionTypes[infusionType] + Upgrades[upgradeLevel];

                var request = new SpawnRequest(finalItemId, quantity, durability);

                ValidateMemoryRegions(requestLocation, outputLocation, shellcodeLocation);

                _process.WriteBytes(requestLocation, StructToBytes(request));

                _process.WriteBytes(outputLocation, new byte[16]);

                IntPtr spawnFuncPtr = IntPtr.Add(_process.ds3Base, ItemSpawnOffset);

                IntPtr mapItemManPtr = IntPtr.Add(_process.ds3Base, MapItemManagerOffset);

                IntPtr actualMapItemMan = (IntPtr)_process.ReadUInt64(mapItemManPtr);

                byte[] shellcode = GenerateSpawnShellcode(
                    spawnFuncPtr.ToInt64(),
                    actualMapItemMan.ToInt64(),
                    requestLocation.ToInt64(),
                    outputLocation.ToInt64()
                );

                _process.WriteBytes(shellcodeLocation, shellcode);
                _process.RunThread(shellcodeLocation);

            }
            finally
            {
                _process.WriteBytes(requestLocation, new byte[Marshal.SizeOf<SpawnRequest>()]);
                _process.WriteBytes(outputLocation, new byte[16]);
                _process.WriteBytes(shellcodeLocation, new byte[50]);
            }
        }

        private void ValidateMemoryRegions(IntPtr requestLocation, IntPtr outputLocation, IntPtr shellcodeLocation)
        {
            if (!IsMemoryRegionFree(requestLocation, Marshal.SizeOf<SpawnRequest>()))
                throw new Exception($"Memory at request location is not free");

            if (!IsMemoryRegionFree(outputLocation, 16))
                throw new Exception($"Memory at output location is not free");

            if (!IsMemoryRegionFree(shellcodeLocation, 50))
                throw new Exception($"Memory at shellcode location is not free");
        }

        private bool IsMemoryRegionFree(IntPtr address, int size)
        {
            try
            {
                byte[] currentMemory = _process.ReadBytes(address, size);
                return currentMemory.All(b => b == 0);
            }
            catch
            {
                return false;
            }
        }

        private byte[] GenerateSpawnShellcode(long spawnFunc, long mapItemMan, long requestPtr, long outputPtr)
        {
            var shellcode = new List<byte>
            {
                0x48, 0x83, 0xEC, 0x28,  // sub rsp, 28h
                0x48, 0xB9               // mov rcx, mapItemMan
            };
            shellcode.AddRange(BitConverter.GetBytes(mapItemMan));
            shellcode.AddRange(new byte[] { 0x48, 0xBA });  // mov rdx, requestPtr
            shellcode.AddRange(BitConverter.GetBytes(requestPtr));
            shellcode.AddRange(new byte[] { 0x49, 0xB8 });  // mov r8, outputPtr
            shellcode.AddRange(BitConverter.GetBytes(outputPtr));
            shellcode.AddRange(new byte[] { 0x48, 0xB8 });  // mov rax, spawnFunc
            shellcode.AddRange(BitConverter.GetBytes(spawnFunc));
            shellcode.AddRange(new byte[]
            {
                0xFF, 0xD0,              // call rax
                0x48, 0x83, 0xC4, 0x28,  // add rsp, 28h
                0xC3                     // ret
            });

            return shellcode.ToArray();
        }

        private byte[] StructToBytes<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }
}
