using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DS3Tool
{
    public class LaunchUtils
    {
        public static string promptForFile(string startFolder, string filter)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.InitialDirectory = startFolder;
            d.Filter = filter;
            if (d.ShowDialog() != true)
            {
                return null;
            }
            return d.FileName;
        }

        public static bool launchGame()
        {//TODO: get game dir from steam files. see https://github.com/soulsmods/ModEngine2/blob/main/launcher/steam_app_path.cpp
            try
            {
                string exename = @"eldenring.exe";
                string path = exename;
                string dir = "";

                string dirGuess1 = @"C:\Program Files (x86)\Steam\steamapps\common\ELDEN RING\Game";
                string dirGuess2 = @"D:\Steam\steamapps\common\ELDEN RING\Game";
                string dirGuess3 = @"D:\SteamLibrary\steamapps\common\ELDEN RING\Game";

                string pathGuess1 = System.IO.Path.Combine(dirGuess1, exename);
                string pathGuess2 = System.IO.Path.Combine(dirGuess2, exename);
                string pathGuess3 = System.IO.Path.Combine(dirGuess3, exename);

                if (File.Exists(path))
                {
                    Utils.debugWrite("Game is in working dir");
                }
                else if (File.Exists(pathGuess1))
                {
                    Utils.debugWrite("Game is at default steam location");
                    path = pathGuess1;
                    dir = dirGuess1;
                }
                else if (File.Exists(pathGuess2))
                {
                    Utils.debugWrite(@"Game is in D:\Steam");
                    path = pathGuess2;
                    dir = dirGuess2;
                }
                else if (File.Exists(pathGuess3))
                {
                    Utils.debugWrite(@"Game is on D:\SteamLibrary");
                    path = pathGuess3;
                    dir = dirGuess3;
                }

                if (!File.Exists(path))
                {
                    MessageBox.Show(@"Please find eldenring.exe. This is normally found in C:\Program Files (x86)\Steam\steamapps\common\ELDEN RING\Game but could be somewhere else if you moved the steam library. (If you run the tool from that folder, you won't need to browse.)", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    string filter = exename + "|" + exename;
                    path = promptForFile(null, filter);
                    if (string.IsNullOrEmpty(path)) { return false; }
                    dir = System.IO.Path.GetDirectoryName(path);
                    exename = System.IO.Path.GetFileName(path);
                    Utils.debugWrite("Will use selected path " + path);
                }
                var psi = new ProcessStartInfo(path);
                psi.EnvironmentVariables["SteamAppId"] = "1245620";
                psi.UseShellExecute = false;
                psi.WorkingDirectory = dir;
                Process.Start(psi);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

#if NO_WPF
    //alternative/dummy implementations for console tools
    enum MessageBoxButton { OK }
    enum MessageBoxImage { Information }
    class MessageBox
    {
        public static void Show(string str = "", string str2 = "", MessageBoxButton b = MessageBoxButton.OK, MessageBoxImage i = MessageBoxImage.Information)
        {
            Console.WriteLine($"{str2} {str}");
        }
    }
    class OpenFileDialog
    {
        public string InitialDirectory { get; set; }
        public string Filter { get; set; }
        public string FileName { get; set; }
        public bool? ShowDialog() { return null; }
    }
#endif
}
