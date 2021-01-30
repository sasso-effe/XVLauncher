using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics; // For debug only
using System.Net;
using System.Threading.Tasks;
using XVLauncher.Resources;

namespace XVLauncher
{
    /// <summary>
    /// Class for handling updates with git-based repositories.
    /// </summary>
    public class UpdateHandler
    {
        /// <summary>
        /// The instance of MainWindow that contains GUI elements which show update progress and status.
        /// </summary>
        protected MainWindow Window;
        private WebClient Client;
        private dynamic Res, Commits, Infos;
        private string Link, TargetCommit, Tag;

        /// <summary>
        /// Constructor of the UpdateHandler class.
        /// </summary>
        /// <param name="window">The window where the updater will be called.</param>
        public UpdateHandler(MainWindow window)
        {
            Window = window;
        }

        /// <summary>
        /// Check if it there is a new release on GitLab repo.
        /// </summary>
        /// <returns>true if there is an update available, false otherwise.</returns>
        public async Task<bool> CheckUpdateAvailability()
        {
            if (Properties.Settings.Default.CurrentCommit != (await GetLatestRelease()).Commit)
            {
                Window.infoLabel.Content = "There's an update avaible.";
                return true;
            }
            return false;
        }

        /// <summary>
        /// Compares the currently stored commit id with a target commit id and returns a tuple of list with the old paths and new paths of changed files.
        /// </summary>
        /// <param name="targetCommit">The target commit id using for comparing.</param>
        /// <returns></returns>
        public async Task<(List<string> oldPath, List<string> newPath)> Compare(string targetCommit)
        {
            Window.button.IsEnabled = false;
            // The current latest commit SHA. It should be stored in the Program settings.
            string Current = Properties.Settings.Default.CurrentCommit;
            // The target latest commit SHA. It can be retrieved with the "GetLatestRelease" method.
            string Target = targetCommit;

            // This is the list of items modified between the commits. 
            // Probably all the "oldPaths" should be deleted, whilst all the "newPaths" redownloaded.
            // The list of old paths retrieved.
            List<string> oldPaths = new List<string>();
            // The list of new paths retrieved.
            List<string> newPaths = new List<string>();

            // The url for the API request.
            string api_request = $"https://gitlab.com/api/v4/projects/{Properties.Settings.Default.ProjectID}/repository/compare?ref_name=Release&from={Current}&to={Target}&per_page=1000";
            Uri api_request_uri = new Uri(api_request);

            SetUpClient(api_request);

            // Getting the commits list
            Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadCommitsListEventHandler);
            await Client.DownloadStringTaskAsync(api_request_uri);
            Client.Dispose();
            SetUpClient(api_request);
            //Getting the id list for the commits found.
            List<string> ids = new List<string>();
            foreach (var c in Commits)
            {
                dynamic val = JsonConvert.DeserializeObject(c.ToString());
                ids.Add(val.id.ToString());
            }
            //Utility for the GUI update.
            int progress = 0;
            int full = ids.Count;
            foreach (var id in ids)
            {
                Debug.WriteLine($"Checking commit {id}");
                int page = 1;
                string url = $"https://gitlab.com/api/v4/projects/{Properties.Settings.Default.ProjectID}/repository/commits/" + id + $"/diff?per_page=1000000&page={page}";
                Client.BaseAddress = url;
                Debug.WriteLine($"Url is: {url}");
                Client.Headers.Add("Content-Type:application/json; charset=utf-8"); //Content-Type  
                Client.Headers.Add("Accept:application/json");
                Client.Headers["Private-Token"] = Properties.Resources.AccessToken;
                //await Client.DownloadStringTaskAsync(api_request_uri);
                Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadCommitInfoEventHandler);
                await Client.DownloadStringTaskAsync(url);
                progress++;
                Window.Dispatcher.Invoke(() =>
                {
                    double percentage = (float)progress / full * 100;
                    Window.infoLabel.Content = String.Format("Comparing the differences between the old release and the new one... {0:0.##}%", percentage);
                    Window.UpdateBarProgress(percentage);
                });
                while (Infos.Count > 0)
                {
                    foreach (var info in Infos)
                    {
                        var desInfo = JsonConvert.DeserializeObject(info.ToString());
                        string oldPath = desInfo.old_path.ToString();
                        string newPath = desInfo.new_path.ToString();
                        if ((bool)desInfo.new_file)
                        {
                            if (!newPaths.Contains(newPath))
                                newPaths.Add(newPath);
                        }
                        else if ((bool)desInfo.renamed_file)
                        {
                            if (!newPaths.Contains(newPath))
                                newPaths.Add(newPath);
                            if (newPaths.Contains(oldPath))
                                newPaths.Remove(oldPath);
                            if (!oldPaths.Contains(oldPath))
                                oldPaths.Add(oldPath);
                        }
                        else if ((bool)desInfo.deleted_file)
                        {
                            if (newPaths.Contains(newPath))
                                newPaths.Remove(newPath);
                            if (!oldPaths.Contains(oldPath))
                                oldPaths.Add(oldPath);
                        }
                        else
                        {
                            if (!newPaths.Contains(newPath))
                                newPaths.Add(newPath);
                        }
                    }

                    page += 1;
                    url = $"https://gitlab.com/api/v4/projects/{Properties.Settings.Default.ProjectID}/repository/commits/" + id + $"/diff?per_page=1000000&page={page}";
                    await Client.DownloadStringTaskAsync(url);
                }
            }

            oldPaths.Sort();
            newPaths.Sort();
            string results = "";
            foreach (string s in newPaths)
            {
                results += "\"" + s + "\"" + "\n";
            }
            Window.Dispatcher.Invoke(() =>
            {
                Debug.WriteLine(results);
            });
            Client.Dispose();
            return (oldPaths, newPaths);
        }

        private void SetUpClient(string api_request)
        {

            Client = new WebClient
            {
                BaseAddress = api_request
            };
            Client.Headers.Add("Content-Type:application/json"); //Content-Type  
            Client.Headers.Add("Accept:application/json");
            Client.Headers["Private-Token"] = Properties.Resources.AccessToken;
        }

        private void DownloadCommitsListEventHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //TODO: show error on label
                PhpManager.ReportError(e.Error.Message);
            }
            else
            {
                Res = JsonConvert.DeserializeObject(e.Result.ToString());
                Commits = JsonConvert.DeserializeObject(Res.commits.ToString());
            }
        }

        private void DownloadCommitInfoEventHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //TODO: show error on label
                PhpManager.ReportError(e.Error.Message);
            }
            else
            {
                Infos = JsonConvert.DeserializeObject(e.Result.ToString());
            }
        }

        private async Task SetLatestRelease()
        {
            string Url = $"https://gitlab.com/api/v4/projects/{Properties.Settings.Default.ProjectID}/releases";
            string Link = "";
            string TargetCommit = "";
            string Tag = "";
            using (var client = new System.Net.WebClient()) //WebClient  
            {
                client.BaseAddress = Url;
                client.Headers.Add("Content-Type:application/json"); //Content-Type  
                client.Headers.Add("Accept:application/json");
                client.Headers["Private-Token"] = Properties.Resources.AccessToken;

                // Getting releases list
                dynamic res = JsonConvert.DeserializeObject(await client.DownloadStringTaskAsync(Url));

                // Getting the latest release (it will always be the first item in the retrieved collection)
                dynamic latest = JsonConvert.DeserializeObject(res[0].ToString());
                dynamic latestCommit = JsonConvert.DeserializeObject(latest.commit.ToString());
                TargetCommit = latestCommit.id.ToString();
                Tag = latest.tag_name.ToString();
                //Getting the direct link to the .zip asset of the release
                dynamic latestZip = JsonConvert.DeserializeObject(latest.assets.sources[0].ToString());
                Link = latestZip.url.ToString();
                string results = latest.ToString();
            }
            this.Link = Link;
            this.TargetCommit = TargetCommit;
            this.Tag = Tag;
        }

        /// <summary>
        /// Retrieves the latest github release informations.
        /// </summary>
        /// <returns>last release download link, last commit name, last release tag.</returns>
        public async Task<(string Link, string Commit, string Tag)> GetLatestRelease()
        {
            if (this.TargetCommit == null)
            {
                await SetLatestRelease();
            }
            return (this.Link, this.TargetCommit, this.Tag);
        }
    }
}

