using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DS3Tool
{
    public class FileUtils
    {
        public static List<string[]> importGenericTextResource(string name, char separator = '\t')
        {
            var ret = new List<string[]>();
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        var spl = line.Split(separator);
                        if (spl.Length < 1) { continue; }
                        ret.Add(spl);
                    }
                }
                return ret;
            }
            catch
            {
                return ret;
            }
        }

        public static string getFnameInAppdata(string fname, string appName, bool localAppData = false)
        {
            //will create dir if needed, otherwise return a full filename.
            var appData = Environment.GetFolderPath(localAppData ? Environment.SpecialFolder.LocalApplicationData : Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, appName);
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            return Path.Combine(appFolder, fname);
        }

        public static DateTime getFileDate(string fname)
        {
            //will create a file if needed
            var ret = new DateTime(0);
            try
            {
                if (File.Exists(fname))
                {
                    ret = File.GetLastWriteTime(fname);
                }
                else
                {
                    File.Create(fname).Dispose();
                }
            }
            catch (Exception ex) { Utils.debugWrite(ex.ToString()); }
            return ret;
        }

        public static bool setFileDate(string fname)
        {
            try
            {
                File.SetLastWriteTime(fname, DateTime.Now);
                return true;
            }
            catch (Exception ex) { Utils.debugWrite(ex.ToString()); }
            return false;
        }

        public static void removeFile(string fname)
        {
            try
            {
                if (File.Exists(fname)) { File.Delete(fname); }
            }
            catch (Exception ex) { Utils.debugWrite(ex.ToString()); }
        }

    }
}
