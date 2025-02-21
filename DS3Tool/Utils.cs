using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace DS3Tool
{
    public class Utils
    {
        public static void debugWrite(string str)
        {
            Trace.WriteLine(str);
        }

        public static string CapitalizeFirst(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
}