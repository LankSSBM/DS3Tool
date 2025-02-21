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

        public static int compVers(string a, string b)
        {
            try
            {
                return new Version(a).CompareTo(new Version(b));
            }
            catch (Exception ex)
            {
                debugWrite(ex.ToString());
                return -100;
            }
        }

        public static string doHTTPReq(string url, string userAgent)
        {
            string ret = null;
            try
            {
                var request = WebRequest.Create(url);
                ((HttpWebRequest)request).UserAgent = userAgent;
                var response = request.GetResponse();
                debugWrite($"StatusCode: {((HttpWebResponse)response).StatusCode}");
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);

                ret = reader.ReadToEnd();
                reader.Close(); //closes the stream and the response
            }
            catch (Exception ex)
            {
                debugWrite(ex.ToString());
            }
            return ret;
        }

        public static int checkVerAgainstURL(string url, string userAgent, string appVer)
        {
            //appVer -> Application.ProductVersion
            //URL should have a version number followed by a newline. later lines are ignored.

            //1 -> newer ver available.
            //0 -> current.
            //-1 -> this version is newer.
            //-100 -> error
            var vInfo = doHTTPReq(url, userAgent);
            if (string.IsNullOrEmpty(vInfo)) { return -100; }
            debugWrite(vInfo);
            try
            {
                var r = new StringReader(vInfo);
                var ver = r.ReadLine();
                if (ver != null) { return compVers(ver, appVer); }
            }
            catch (Exception ex)
            {
                debugWrite(ex.ToString());
            }
            return -100;
        }

        public static string getFnameInAppdata(string fname, string appName, bool localAppData = false)
        {//will create dir if needed, otherwise return a full filename.
            var appData = Environment.GetFolderPath(localAppData ? Environment.SpecialFolder.LocalApplicationData : Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, appName);
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            return Path.Combine(appFolder, fname);
        }

        public static DateTime getFileDate(string fname)
        {//will create a file if needed
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
            catch (Exception ex) { debugWrite(ex.ToString()); }
            return ret;
        }
        public static bool setFileDate(string fname)
        {
            try
            {
                File.SetLastWriteTime(fname, DateTime.Now);
                return true;
            }
            catch (Exception ex) { debugWrite(ex.ToString()); }
            return false;
        }
        public static void removeFile(string fname)
        {
            try
            {
                if (File.Exists(fname)) { File.Delete(fname); }
            }
            catch (Exception ex) { debugWrite(ex.ToString()); }
        }

        public static string CapitalizeFirst(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

    }

    public class StringWrap
    {
        public string DisplayStr { get; set; } = "";
        public object o = null;
        public override string ToString() { return DisplayStr; }
    }
}