using System;
using System.Threading.Tasks;
using XVLauncher.Resources;

namespace XVLauncher
{
    /// <summary>
    /// Abstract class that provides the template method Download(). It is implemented by <see cref="WebDownloader"/> and <see cref="MegaDownloader"/>.
    /// </summary>
    abstract class Downloader
    {

        protected readonly string url;
        protected readonly MainWindow window;
        /// <summary>
        /// Constructor method.
        /// </summary>
        /// <param name="window"><see cref="MainWindow"/> instance where are placed GUI elements to be updated to show progress.</param>
        /// <param name="url"></param>
        public Downloader(MainWindow window, string url)
        {
            this.url = url;
            this.window = window;
        }

        /// <summary>
        /// Template method that only handle exceptions and do some simples GUI changes.<br/>
        /// Real Download implementation is delegated to sublasses that must implement the abstract method <see cref="DownloadImplementation"/>.
        /// </summary>
        /// <returns></returns>
        public async Task Download()
        {

            try
            {
                await DownloadImplementation();
            }
            catch (Exception ex)
            {
                window.infoLabel.Content = String.Format(Properties.Langs.Lang.unable_download, ex.Message);
                PhpManager.ReportError(String.Format("Error in {0}.DownloadImplementation: {1}", GetType().Name, ex.Message));
                window.UpdateBarProgress(0);
                return;
            }
            window.infoLabel.Content = Properties.Langs.Lang.download_completed;
        }

        /// <summary>
        /// Abstract method to be implemented by subclasses, that must provide download implementation, possibly asynchronously. <br/>
        /// It is called by the template method <see cref="Download"/>
        /// </summary>
        /// <returns></returns>
        protected abstract Task DownloadImplementation();

        public long GetFileSize(string url)
        {
            long size = -1;
            var req = System.Net.WebRequest.Create(url);
            req.Method = "HEAD";
            using (var resp = req.GetResponse()) //WebClient  
            {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLenght))
                    size = ContentLenght;
            }
            return size;
        }

        protected void UpdateUnknownProgress(long bytes)
        {
            var mb = bytes / 1048576f;
            var unit = "MB";
            if (mb > 1024)
            {
                mb = mb / 1024f;
                unit = "GB";
            }
            window.Dispatcher.Invoke(() =>
            {
                window.infoLabel.Content = String.Format(Properties.Langs.Lang.download_progress_unknown, mb, unit, "2GB");
            });
        }

        protected void ShowProgress(double x)
        {
            window.Dispatcher.Invoke(() =>
            {
                window.infoLabel.Content = String.Format(Properties.Langs.Lang.download_progress, x);
                window.UpdateBarProgress(x);
            });

        }

        protected void ShowError(string error, bool buttonEnabled = false)
        {
            window.Dispatcher.Invoke(() =>
            {
                window.infoLabel.Content = error;
                window.button.IsEnabled = buttonEnabled;
            });
        }

    }
}
