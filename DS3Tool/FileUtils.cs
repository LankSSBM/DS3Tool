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
    }
}
