using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using XVLauncher.Resources;

namespace XVLauncher
{
    /// <summary>
    /// Subclass of <see cref="Downloader"/> that handle updates from GitLab repository.
    /// </summary>
    class UpdateDownloader : Downloader
    {
        private readonly List<string> oldPath, newPath;
        private readonly UpdateHandler handler;

        /// <summary>
        /// Constructor method.
        /// </summary>
        /// <param name="window"><see cref="MainWindow"/> instance where are placed GUI elements to be updated to show progress.</param>
        /// <param name="url"></param>
        /// <param name="oldPath">List of path that do not exists in the new update, because file have been mooved or deleted.</param>
        /// <param name="newPath">List of new path</param>
        /// <param name="handler">An instance of <see cref="UpdateHandler"/></param>
        public UpdateDownloader(MainWindow window, string url, List<string> oldPath, List<string> newPath, UpdateHandler handler) : base(window, url)
        {
            this.oldPath = oldPath;
            this.newPath = newPath;
            this.handler = handler;
        }


        protected override async Task DownloadImplementation()
        {

            string[] roots = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\" + Properties.Settings.Default.GameDirectory);
            if (roots.Length > 1)
            {
                throw new DirectoryNotFoundException(message: string.Format("Multiple roots:\n{0}", string.Join("\n", roots)));
            }
            if (roots.Length == 0)
            {
                ShowError("There are no files inside your installation directory.");
                throw new DirectoryNotFoundException();
            }
            string root = roots[0];
            foreach (string file in oldPath)
            {
                var fileInfo = new FileInfo(root + "\\" + file);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
            }
            double n_file = newPath.Count;
            int downloaded = 0;
            using (var webClient = new WebClient())
            {
                foreach (string file in newPath)
                {
                    string url = this.url + file;
                    System.Diagnostics.Debug.WriteLine(file);
                    System.Diagnostics.Debug.WriteLine(root + "\\" + file.Replace("/", "\\"));
                    System.Diagnostics.Debug.WriteLine(url);
                    var fileInfo = new FileInfo(root + "\\" + file.Replace("/", "\\"));
                    if (fileInfo.Exists)
                        fileInfo.Delete();
                    if (fileInfo.Directory.Exists == false)
                        fileInfo.Directory.Create();
                    try
                    {
                        await webClient.DownloadFileTaskAsync(url, root + "\\" + file.Replace("/", "\\"));
                    }
                    catch (WebException)
                    {
                        // The file is in newPath but it has been mooved or deleted in a future commit, so it cannot be downloaded, just do nothing
                    }
                    downloaded++;
                    ShowProgress(downloaded / n_file * 100);
                }
            }
            Properties.Settings.Default.Version = (await handler.GetLatestRelease()).Tag;
            Properties.Settings.Default.CurrentCommit = (await handler.GetLatestRelease()).Commit;
            Properties.Settings.Default.Save();
            PhpManager.UpdateVersion((await handler.GetLatestRelease()).Tag);

        }

    }
}
