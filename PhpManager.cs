using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace XVLauncher.Resources
{
    /// <summary>
    /// This class provides methods to dialog with php files. To implement the methods of this class you need a server to host php files and a database.<br/>
    /// Php files and MySQL query to create the appropriate table are provided in the GitHub repository.
    /// </summary>
    static class PhpManager
    {
        /// <summary>
        /// Set it to true if you want to use php files to communicate with your server. Check the documntation for more details.
        /// </summary>
        private static readonly bool USE_PHP = false;
        private static string megaUrl, lastVer;

        /// <summary>
        /// Getter method for <see cref="megaUrl"/>.
        /// </summary>
        /// <returns></returns>
        public static string GetMegaUrl()
        {
            return megaUrl;
        }
        /// <summary>
        /// Getter method for lastVer.
        /// </summary>
        /// <returns>Last version available.</returns>
        public static string GetLastVer()
        {
            return lastVer;
        }

        /// <summary>
        /// Assign to user a new auto-increment ID.
        /// </summary>
        /// <param name="version">current game version tag.</param>
        /// <returns>ID.</returns>
        public static int GetID(string version)
        {
            if (!USE_PHP) return -1;
            // if USE_PHP is true, then get id:
            String ver = HttpUtility.UrlEncode(version);
            string response = SendPost(SecurityManager.GetInstance().GetPath() + "get_id.php", String.Format("version={0}", ver));

            string[] subs = response.Split('=');
            return (subs[0] == "id") ? Int32.Parse(subs[1]) : -1;
        }

        /// <summary>
        /// Update user current version in the database.<br/>
        /// Since in the database the column last_date is setted as ON UPDATE CURRENT_TIMESTAMP, this method can also be used to update that column.<br/>
        /// For example, it can be called whenever the game is launched, so the database will register the last time each id has launched the game.<br/>
        /// It can be useful to know if someone is still playing the game (using the application/using whatever this launcher is used to) after months or years.
        /// </summary>
        /// <param name="newVer">User current version.</param>
        public static void UpdateVersion(string newVer)
        {
            if (!USE_PHP) return;
            // if USE_PHP is true, then update version:
            newVer = FormatVersionTag(newVer);
            String id = HttpUtility.UrlEncode(Properties.Settings.Default.ID.ToString());
            String ver = HttpUtility.UrlEncode(newVer);
            SendPost(SecurityManager.GetInstance().GetPath() + "update_ver.php", String.Format("version={0}&id={1}", ver, id));
        }

        /// <summary>
        /// Try to contact server and, if the connection is successful, set url to mega archive in <c>megaurl</c>
        /// and the tag of the last version available in <c>lastVer</c>.<br/>
        /// This method should not be used if the game is stored in a GitLab repository, in that case version tag can be retrieved with <see cref="UpdateHandler.GetLatestRelease"/>, while <c>megaurl</c> is no more useful.
        /// </summary>
        /// <returns><c>true</c> if the conncection is successful, <c>false</c> otherwise.</returns>
        public static async Task<bool> FirstContact()
        {
            if (!USE_PHP) return false;
            // if USE_PHP is true, then contact server:
            try
            {
                string responseBody = await new HttpClient().GetStringAsync(SecurityManager.GetInstance().GetPath() + "last_ver.php");
                string[] temp = responseBody.Split('\n');
                megaUrl = temp[0];
                lastVer = temp[1];
                return true;
            }
            catch (HttpRequestException e)
            {
                //TODO: handle exception
                Debug.WriteLine("\nException caught!\nMessage: {0}", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Store error message in last_error column.<br/>
        /// Also update error_date column with current timestamp.
        /// </summary>
        /// <param name="error">Error message.</param>
        public static void ReportError(string error)
        {
            if (!USE_PHP) return;
            error = Truncate(error, 256);
            String e = HttpUtility.UrlEncode(error);
            String id = HttpUtility.UrlEncode(Properties.Settings.Default.ID.ToString());
            String ver = HttpUtility.UrlEncode(Properties.Settings.Default.Version);
            SendPost(SecurityManager.GetInstance().GetPath() + "send_error.php", String.Format("version={0}&id={1}&error={2}", ver, id, e), false);
        }

        private static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        }


        /// <summary>
        /// Helper method provided by makim on this Stack Overflow thread: https://stackoverflow.com/questions/6960426/c-sharp-xml-documentation-website-link.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="report">In case of Exception, don't call ReportError if it is <c>false</c>. Default is <c>true</c>, but in some cases it can be useful to set it to <c>false</c> to avoid loops.</param>
        /// <returns>webpage content.</returns>
        private static string SendPost(string url, string postData, bool report = true)
        {
            string webpageContent = string.Empty;

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;

                using (Stream webpageStream = webRequest.GetRequestStream())
                {
                    webpageStream.Write(byteArray, 0, byteArray.Length);
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        webpageContent = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                if (report)
                    ReportError(ex.Message);
            }

            return webpageContent;
        }

        private static string FormatVersionTag(string versionTag)
        {
            if (versionTag == null)
            {
                return "na";
            }
            string[] subs = versionTag.Split('.');
            string NewTag = "";
            foreach (string s in subs)
            {
                NewTag += (s.Length == 1) ? ('0' + s + '.') : (s + '.');
            }
            return NewTag.Remove(NewTag.Length - 1);
        }
    }
}
