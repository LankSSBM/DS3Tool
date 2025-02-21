using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

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
        {
            try
            {
                string path = GetDarkSouls3ExePath();

                if (File.Exists(path))
                {
                    Utils.debugWrite("Found DS3 exe at: " + path);
                }
                else
                {
                    Utils.debugWrite("Found DS3 exe at: " + path);
                }

                var psi = new ProcessStartInfo(path);
                //psi.EnvironmentVariables["SteamAppId"] = "1245620";
                psi.UseShellExecute = false;
                //psi.WorkingDirectory = dir;
                Process.Start(psi);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetDarkSouls3ExePath()
        {
            string steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) as string;
            if (string.IsNullOrEmpty(steamPath))
                return null;

            List<string> libraries = new List<string>();

            string steamInstallConfigPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf"); // contains info about steam games install locations
            if (File.Exists(steamInstallConfigPath))
            {

                string[] lines = File.ReadAllLines(steamInstallConfigPath);
                var regex = new Regex(@"""path""\s+""(.+?)"""); // search the config for lines with the text: "path" 

                foreach (string line in lines)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        string path = match.Groups[1].Value.Replace(@"\\", @"\");
                        libraries.Add(path);
                    }
                }

            }

            foreach (string installDir in libraries)
            {
                string gamePath = Path.Combine(installDir, "steamapps", "common", "DARK SOULS III", "Game", "DarkSoulsIII.exe");
                if (File.Exists(gamePath))
                {
                    return gamePath;
                }
            }

            return null;
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
}
