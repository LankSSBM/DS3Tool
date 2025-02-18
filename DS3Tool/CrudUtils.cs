using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DS3Tool;
using System.Threading.Tasks;

namespace DS3Tool
{
    public static class CrudUtils
    {
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int iSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int iSize, int lpNumberOfBytesWritten);
        [DllImport("ntdll.dll")]
        static extern int NtWriteVirtualMemory(IntPtr ProcessHandle, IntPtr BaseAddress, byte[] Buffer, UInt32 NumberOfBytesToWrite, ref UInt32 NumberOfBytesWritten); //TODO: replace all read/write process memory with this and the equivalent read func.


        //all read/write funcs just fail silently, except this one:
        public static bool ReadTest(IntPtr processHandle, IntPtr addr)
        {
            var array = new byte[1];
            var lpNumberOfBytesRead = 1;
            return ReadProcessMemory(processHandle, addr, array, 1, ref lpNumberOfBytesRead) && lpNumberOfBytesRead == 1;
        }

        public static void ReadTestFull(IntPtr processHandle, IntPtr addr)
        {
            Console.WriteLine($"Testing Address: 0x{addr.ToInt64():X}");

            bool available = ReadTest(processHandle, addr);
            Console.WriteLine($"Availability: {available}");

            if (!available)
            {
                Console.WriteLine("Memory is not readable at this address.");
                return;
            }

            try
            {
                Console.WriteLine($"Int32: {ReadInt32(processHandle, addr)}");
                Console.WriteLine($"Int64: {ReadInt64(processHandle, addr)}");
                Console.WriteLine($"UInt8: {ReadUInt8(processHandle, addr)}");
                Console.WriteLine($"UInt32: {ReadUInt32(processHandle, addr)}");
                Console.WriteLine($"UInt64: {ReadUInt64(processHandle, addr)}");
                Console.WriteLine($"Float: {ReadFloat(processHandle, addr)}");
                Console.WriteLine($"Double: {ReadDouble(processHandle, addr)}");
                Console.WriteLine($"String: {ReadString(processHandle, addr)}");

                byte[] bytes = ReadBytes(processHandle, addr, 16);
                Console.WriteLine("Bytes: " + BitConverter.ToString(bytes));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading memory: " + ex.Message);
            }
        }

        public static int ReadInt32(IntPtr processHandle, IntPtr addr)
        {
            var bytes = ReadBytes(processHandle, addr, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static long ReadInt64(IntPtr processHandle, IntPtr addr)
        {
            var bytes = ReadBytes(processHandle, addr, 8);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static byte ReadUInt8(IntPtr processHandle, IntPtr addr)
        {
            var bytes = ReadBytes(processHandle, addr, 1);
            return bytes[0];
        }

        public static uint ReadUInt32(IntPtr processHandle, IntPtr addr)
        {
            var bytes = ReadBytes(processHandle, addr, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static ulong ReadUInt64(IntPtr processHandle, IntPtr addr)
        {
            var bytes = ReadBytes(processHandle, addr, 8);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static float ReadFloat(IntPtr processHandle, IntPtr addr)
        {
            var bytes = ReadBytes(processHandle, addr, 4);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static double ReadDouble(IntPtr processHandle, IntPtr addr)
        {
            var bytes = ReadBytes(processHandle, addr, 8);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static byte[] ReadBytes(IntPtr processHandle, IntPtr addr, int size)
        {
            var array = new byte[size];
            var lpNumberOfBytesRead = 1;
            ReadProcessMemory(processHandle, addr, array, size, ref lpNumberOfBytesRead);
            return array;
        }

        public static string ReadString(IntPtr processHandle, IntPtr addr, int maxLength = 32)
        {
            var bytes = ReadBytes(processHandle, addr, maxLength * 2);

            int stringLength = 0;
            for (int i = 0; i < bytes.Length - 1; i += 2)
            {
                if (bytes[i] == 0 && bytes[i + 1] == 0)
                {
                    stringLength = i;
                    break;
                }
            }

            if (stringLength == 0)
            {
                stringLength = bytes.Length - (bytes.Length % 2);
            }

            return System.Text.Encoding.Unicode.GetString(bytes, 0, stringLength);
        }

        public static void WriteInt32(IntPtr processHandle, IntPtr addr, int val)
        {
            WriteBytes(processHandle, addr, BitConverter.GetBytes(val));
        }

        public static void WriteUInt32(IntPtr processHandle, IntPtr addr, uint val)
        {
            WriteBytes(processHandle, addr, BitConverter.GetBytes(val));
        }

        public static void WriteFloat(IntPtr processHandle, IntPtr addr, float val)
        {
            WriteBytes(processHandle, addr, BitConverter.GetBytes(val));
        }

        public static void WriteUInt8(IntPtr processHandle, IntPtr addr, byte val)
        {
            var bytes = new byte[] { val };
            WriteBytes(processHandle, addr, bytes);
        }

        public static void WriteBytes(IntPtr processHandle, IntPtr addr, byte[] val, bool useNewWrite = true)
        {
            if (useNewWrite)
            {
                uint written = 0;
                NtWriteVirtualMemory(processHandle, addr, val, (uint)val.Length, ref written); //MUCH faster, <1ms
            }
            else
            {
                WriteProcessMemory(processHandle, addr, val, val.Length, 0); //can take as long as 15ms!
            }
        }

        public static void WriteString(IntPtr processHandle, IntPtr addr, string value, int maxLength = 32)
        {
            var bytes = new byte[maxLength];
            var stringBytes = System.Text.Encoding.Unicode.GetBytes(value);
            Array.Copy(stringBytes, bytes, Math.Min(stringBytes.Length, maxLength));
            WriteBytes(processHandle, addr, bytes);
        }
    }

}
