using CG.Web.MegaApiClient;
using System;
using System.Threading.Tasks;

namespace XVLauncher
{
    /// <summary>
    /// Subclass of <see cref="Downloader"/> that use <see cref="MegaApiClient"/> to download a zipped version of the project from a mega.nz url.
    /// </summary>
    class MegaDownloader : Downloader
    {
        private readonly IMegaApiClient client;
        /// <summary>
        /// Constructor method.
        /// </summary>
        /// <param name="window"><see cref="MainWindow"/> where are placed GUI elements to be updated to show progress.</param>
        /// <param name="url">mega.nz link to download a .zip including the .exe to be launched and all the files needed.</param>
        public MegaDownloader(MainWindow window, string url) : base(window, url)
        {
            client = new MegaApiClient();
        }

        async protected override Task DownloadImplementation()
        {
            await client.LoginAnonymousAsync();
            Uri folderLink = new Uri(url);
            INodeInfo node = client.GetNodeFromLink(folderLink);
            IProgress<double> progressHandler = new Progress<double>(x => ShowProgress(x));
            await client.DownloadFileAsync(folderLink, node.Name, progressHandler);
            await client.LogoutAsync();
        }

    }
}
