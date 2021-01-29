using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace XVLauncher
{
    /// <summary>
    /// Subclass of <see cref="Downloader"/> that use <see cref="WebClient"/> to download a zipped version of the project from a direct download url.
    /// </summary>
    class WebDownloader : Downloader
    {

        private readonly WebClient client;

        /// <summary>
        /// Constructor method.
        /// </summary>
        /// <param name="window"><see cref="MainWindow"/> where are placed GUI elements to be updated to show progress.</param>
        /// <param name="url">direct link to download a .zip including the .exe to be launched and all the files needed.</param>
        public WebDownloader(MainWindow window, string url) : base(window, url)
        {
            client = new WebClient();
        }

        protected async override Task DownloadImplementation()
        {
            var totalsize = GetFileSize(url);
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + Properties.Settings.Default.GameDirectory);
            if (totalsize == -1)
            {
                window.Dispatcher.Invoke(() =>
                {
                    window.SetBarIdle(true);
                });
            }
            System.Diagnostics.Debug.WriteLine(totalsize);
            client.DownloadProgressChanged += (s, e) =>
            {
                if (e.TotalBytesToReceive == -1)
                {
                    UpdateUnknownProgress(e.BytesReceived);
                }
                else
                {
                    ShowProgress(e.ProgressPercentage);
                }
            };
            client.DownloadDataCompleted += (s, e) =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    window.SetBarIdle(false);
                });
                System.Diagnostics.Debug.WriteLine("finished download, disposing client");
                client.Dispose();
            };
            client.OpenRead(url);
            await client.DownloadFileTaskAsync(new Uri(url), Directory.GetCurrentDirectory() + "\\" + Properties.Settings.Default.GameDirectory + ".zip");
        }
    }
}
