using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS3Tool
{
    public class AOBScanner : IDisposable
    {
        [DllImport("ntdll.dll")]
        static extern int NtReadVirtualMemory(IntPtr ProcessHandle, IntPtr BaseAddress, byte[] Buffer, UInt32 NumberOfBytesToRead, ref UInt32 NumberOfBytesRead);

        public uint textOneSize = 0;
        public int textOneAddr = 0;
        public uint textTwoSize = 0;
        public int textTwoAddr = 0;
        public byte[] sectionOne = new byte[0];
        public byte[] sectionTwo = new byte[0];

        public AOBScanner(IntPtr handle, IntPtr baseAddr, int size)
        {
#if !DEBUG
            outputConsole = false;
#endif

            //TODO: switch to .NET 5+ and use PEHeaders?
            //for now: assume two text sections, and aob scan for them, of course.

            var buf = new byte[0x600];
            uint bytesRead = 0;
            NtReadVirtualMemory(handle, baseAddr, buf, (uint)buf.Length, ref bytesRead);
            var dotText = Encoding.ASCII.GetBytes(".text");
            var dummy = new byte[5];
            int textOne = FindBytes(buf, dotText, dummy);
            if (textOne < 0)
            {
                Console.WriteLine("Cannot find text section");
                return;
            }
            textOneSize = BitConverter.ToUInt32(buf, textOne + 8);
            textOneAddr = BitConverter.ToInt32(buf, textOne + 12);
            sectionOne = new byte[textOneSize];
            NtReadVirtualMemory(handle, baseAddr + textOneAddr, sectionOne, textOneSize, ref bytesRead);

            int textTwo = FindBytes(buf, dotText, dummy, textOne + 0x28);
            if (textTwo > 0)
            {
                textTwoSize = BitConverter.ToUInt32(buf, textTwo + 8);
                textTwoAddr = BitConverter.ToInt32(buf, textTwo + 12);
                sectionTwo = new byte[textTwoSize];
                NtReadVirtualMemory(handle, baseAddr + textTwoAddr, sectionTwo, textTwoSize, ref bytesRead);
            }

            Console.ReadLine();
        }

        //originally from https://github.com/Wulf2k/ER-Patcher.git
        //try and keep in sync with https://github.com/kh0nsu/FromAobScan
        public byte[] hs2b(string hex)
        {
            hex = hex.Replace(" ", "");
            hex = hex.Replace("-", "");
            hex = hex.Replace(":", "");

            byte[] b = new byte[hex.Length >> 1];
            for (int i = 0; i <= b.Length - 1; ++i)
            {
                b[i] = (byte)((hex[i * 2] - (hex[i * 2] < 58 ? 48 : (hex[i * 2] < 97 ? 55 : 87))) * 16 + (hex[i * 2 + 1] - (hex[i * 2 + 1] < 58 ? 48 : (hex[i * 2 + 1] < 97 ? 55 : 87))));
            }
            return b;
        }

        public byte[] hs2w(string hex)
        {
            hex = hex.Replace(" ", "");
            hex = hex.Replace("-", "");
            hex = hex.Replace(":", "");

            byte[] wild = new byte[hex.Length >> 1];
            for (int i = 0; i <= wild.Length - 1; ++i)
            {
                if (hex[i * 2].Equals('?'))
                {
                    wild[i] = 1;
                }
            }
            return wild;
        }

        public int FindBytes(byte[] buf, byte[] find, byte[] wild, int startIndex = 0, int lastIndex = -1)
        {
            if (buf == null || find == null || buf.Length == 0 || find.Length == 0 || find.Length > buf.Length) return -1;
            if (lastIndex < 1) { lastIndex = buf.Length - find.Length; }
            for (int i = startIndex; i < lastIndex + 1; i++)
            {
                if (buf[i] == find[0])
                {
                    for (int m = 1; m < find.Length; m++)
                    {
                        if ((buf[i + m] != find[m]) && (wild[m] != 1)) break;
                        if (m == find.Length - 1) return i;
                    }
                }
            }
            return -1;
        }

        public bool outputConsole = true;

        public int findAddr(byte[] buf, int blockVirtualAddr, string find, string desc, int readoffset32 = -1000, int nextInstOffset = -1000, int justOffset = -1000, int startIndex = 0, bool singleMatch = true, Action<int> callback = null)
        {//TODO: for single match and non-zero start index, try zero start index if no match is found?
            int count = 0;

            byte[] fb = hs2b(find);
            byte[] fwb = hs2w(find);

            int index = startIndex;

            int result = -1;

            do
            {
                index = FindBytes(buf, fb, fwb, index);
                if (index != -1)
                {
                    count++;
                    int rva = index + blockVirtualAddr;
                    result = rva;
                    string output = desc + " found at index " + index + " offset hex " + rva.ToString("X2");

                    if (readoffset32 > -1000)
                    {
                        int index32 = index + readoffset32;
                        var val = BitConverter.ToInt32(buf, index32);
                        result = val;
                        output += " raw val " + val.ToString("X2");
                        if (nextInstOffset > -1000)
                        {
                            int next = blockVirtualAddr + index + nextInstOffset + val;
                            result = next;
                            output += " final offset " + next.ToString("X2");
                        }
                    }

                    if (justOffset > -1000)
                    {
                        result = rva + justOffset;
                        output += " with offset " + (rva + justOffset).ToString("X2");
                    }

                    if (outputConsole) { Console.WriteLine(output); }
                    index += fb.Length; //keep searching in case there's multiple.
                }
                if (index != -1 && callback != null) { callback(result); }
            }
            while (index != -1 && !singleMatch);
            if (0 == count) { Console.WriteLine("Nothing found for " + desc); }
            return result;
        }

        public void Dispose()
        {
            sectionOne = null;
            sectionTwo = null;
        }
    }


}
