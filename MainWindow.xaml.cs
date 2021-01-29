using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics; // For debug only
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using XVLauncher.Resources;

namespace XVLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private readonly List<BitmapImage> Images = new List<BitmapImage>();
        private int ImageNumber = 0;
        private readonly DispatcherTimer PictureTimer = new DispatcherTimer();
        private readonly DispatcherTimer UpdateTimer = new DispatcherTimer();
        private UpdateHandler updateHandler;
        private bool inOptions, inNotice = false;
        private readonly string FILE_NAME = Properties.Settings.Default.GameDirectory;

        /// <summary>
        /// This method is called when the program starts.<br/>
        /// It checks if the user has an ID assigned, if not it try to get a new ID from the database and assign it to the user.<br/>
        /// Then check if there are fonts to be installed and, if it is the case, installs them.<br/>
        /// Finally, initialise the GUI.
        /// </summary>
        public MainWindow()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (Properties.Settings.Default.ID == -1)
            {
                Properties.Settings.Default.ID = PhpManager.GetID(Properties.Settings.Default.Version);
                Properties.Settings.Default.Save();
            }
            InitializeComponent();
        }

        /// <summary>
        /// It is called when the GUI is loaded. It implements the main logic of the program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string myVer = Properties.Settings.Default.Version;
            game_version_label.Content = String.Format(Properties.Langs.Lang.game_version, myVer);
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            launcher_version_label.Content = String.Format(Properties.Langs.Lang.launcher_version, version);

            updateHandler = new UpdateHandler(this);
            Boolean isServerReachable = true;
            Boolean isToUpdate;
            try
            {
                isToUpdate = await updateHandler.CheckUpdateAvailability();
            }
            catch (Exception ex)
            {
                isServerReachable = false;
                isToUpdate = false;
                PhpManager.ReportError("updateHandler.CheckUpdateAvailability(); raised an Exception:\n" + ex.Message);
            }
            await MainLogic(isToUpdate, isServerReachable, myVer);

            //Loads all images in the current directory into a list 
            DirectoryInfo dir_info = new DirectoryInfo(Environment.CurrentDirectory + "/Resources");
            foreach (FileInfo file_info in dir_info.GetFiles())
            {
                if ((file_info.Extension.ToLower() == ".jpg") ||
                    (file_info.Extension.ToLower() == ".png"))
                {
                    Images.Add(new BitmapImage(new Uri(file_info.FullName)));
                }
            }

            // Sets first image as source for bg
            bg.Source = Images[0];
            BottomBarBg.ImageSource = Images[0];
            TopBarBg.ImageSource = Images[0];

            //Initializing Timer for slideshow
            PictureTimer.Interval = TimeSpan.FromSeconds(10);
            PictureTimer.Tick += Tick;
            PictureTimer.Start();

            //Initializing the update timer
            UpdateTimer.Interval = TimeSpan.FromMilliseconds(10);
            UpdateTimer.Tick += Update;
            UpdateTimer.Start();

            InitSaveButtons();
            id_label.Content = "ID: " + Properties.Settings.Default.ID;

            //Hide splash
            splash.Visibility = Visibility.Hidden;
        }

        private async Task MainLogic(bool isToUpdate, bool isServerReachable, string myVer)
        {
            if (IsGameInstalled())
            {
                // Yes, it is installed. There is an update available?
                if (isToUpdate)
                {
                    // Yes, there is an update. So get info about the last release, and change the button into an Update Button.
                    (string url, string commit, string tag) = await updateHandler.GetLatestRelease();

                    GradientStopCollection c = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(255, 27, 134, 255), 0),
                        new GradientStop(Color.FromArgb(255, 55, 170, 255), 0.3),
                        new GradientStop(Color.FromArgb(255, 55, 170, 255), 0.7),
                        new GradientStop(Color.FromArgb(255, 27, 134, 255), 1.0)
                    };
                    LinearGradientBrush b = new LinearGradientBrush(c)
                    {
                        StartPoint = new Point(0.5, 0),
                        EndPoint = new Point(0.5, 1)
                    };
                    this.button.Background = b;
                    this.button.Content = Properties.Langs.Lang.update;
                    RemoveRoutedEventHandlers(this.button, Button.ClickEvent);
                    this.button.Click += async (s, ee) =>
                    {
                        (List<string> oldPath, List<string> newPath) = await updateHandler.Compare(commit);
                        await new UpdateDownloader(this, String.Format(Properties.Resources.UpdateUrl, tag), oldPath, newPath, updateHandler).Download();
                        InitPlayButton();
                    };
                    this.infoLabel.Content = String.Format(Properties.Langs.Lang.version_available, tag);
                }
                else
                // No, the game is installed but there are not any updates. So initialise the Play Button.
                {
                    this.infoLabel.Content = String.Format(Properties.Langs.Lang.version, myVer);
                    InitPlayButton();
                    if (isServerReachable)
                        this.infoLabel.Content += Properties.Langs.Lang.last_version_av;
                    else
                        this.infoLabel.Content += Properties.Langs.Lang.update_not_possible;
                }

            }
            // No, the game is not installed. Is it possible to connect to the repository?
            else if (isServerReachable)
            {
                // Yes, it is possible to connect to the repository. So, initialize Download Button.
                RemoveRoutedEventHandlers(this.button, Button.ClickEvent);
                this.button.Click += DownloadAndInstall;
                this.infoLabel.Content = "";
                GradientStopCollection c = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(255, 27, 134, 255), 0),
                        new GradientStop(Color.FromArgb(255, 55, 170, 255), 0.3),
                        new GradientStop(Color.FromArgb(255, 55, 170, 255), 0.7),
                        new GradientStop(Color.FromArgb(255, 27, 134, 255), 1.0)
                    };
                LinearGradientBrush b = new LinearGradientBrush(c)
                {
                    StartPoint = new Point(0.5, 0),
                    EndPoint = new Point(0.5, 1)
                };
                this.button.Background = b;
                //delete eventually corrupeted archive
                if (File.Exists(Directory.GetCurrentDirectory() + "\\" + FILE_NAME + ".zip"))
                {
                    File.Delete(Directory.GetCurrentDirectory() + "\\" + FILE_NAME + ".zip");
                }
            }
            else
            {
                // No. The game is not installed and it is not possible to install it. Nothing can be done at the moment.
                this.BarGrid.Visibility = Visibility.Collapsed;
                this.button.Visibility = Visibility.Collapsed;
                this.infoLabel.Content = Properties.Langs.Lang.download_not_possible;
            }
        }

        private void Update(object sender, System.EventArgs e)
        {
            BottomBarBg.ImageSource = bg.Source;
            BottomBarBg.Opacity = bg.Opacity;
            TopBarBg.ImageSource = bg.Source;
            TopBarBg.Opacity = bg.Opacity;
        }
        /// <summary>
        /// Tick on the dispatchtimer called every 10 seconds. Used to update background Images.
        /// </summary>
        private void Tick(object sender, System.EventArgs e)
        {
            ImageNumber = (ImageNumber + 1) % Images.Count;
            DisplayNextImage(bg);
        }

        /// <summary>
        /// Animation code for the image changing
        /// </summary>
        private void DisplayNextImage(Image img)
        {
            const double transition_time = 0.9;
            Storyboard sb = new Storyboard();

            // ***************************
            // Animate Opacity 1.0 --> 0.0
            // ***************************
            DoubleAnimation fade_out = new DoubleAnimation(1.0, 0.0,
                TimeSpan.FromSeconds(transition_time))
            {
                BeginTime = TimeSpan.FromSeconds(0)
            };

            // Use the Storyboard to set the target property.
            Storyboard.SetTarget(fade_out, img);
            Storyboard.SetTargetProperty(fade_out,
                new PropertyPath(Image.OpacityProperty));

            // Add the animation to the StoryBoard.
            sb.Children.Add(fade_out);


            // *********************************
            // Animate displaying the new image.
            // *********************************
            ObjectAnimationUsingKeyFrames new_image_animation =
                new ObjectAnimationUsingKeyFrames
                {
                    // Start after the first animation has finisheed.
                    BeginTime = TimeSpan.FromSeconds(transition_time)
                };

            // Add a key frame to the animation.
            // It should be at time 0 after the animation begins.
            DiscreteObjectKeyFrame new_image_frame =
                new DiscreteObjectKeyFrame(Images[ImageNumber], TimeSpan.Zero);
            new_image_animation.KeyFrames.Add(new_image_frame);

            // Use the Storyboard to set the target property.
            Storyboard.SetTarget(new_image_animation, img);
            Storyboard.SetTargetProperty(new_image_animation,
                new PropertyPath(Image.SourceProperty));

            // Add the animation to the StoryBoard.
            sb.Children.Add(new_image_animation);


            // ***************************
            // Animate Opacity 0.0 --> 1.0
            // ***************************
            // Start when the first animation ends.
            DoubleAnimation fade_in = new DoubleAnimation(0.0, 1.0,
                TimeSpan.FromSeconds(transition_time))
            {
                BeginTime = TimeSpan.FromSeconds(transition_time)
            };

            // Use the Storyboard to set the target property.
            Storyboard.SetTarget(fade_in, img);
            Storyboard.SetTargetProperty(fade_in,
                new PropertyPath(Image.OpacityProperty));

            // Add the animation to the StoryBoard.
            sb.Children.Add(fade_in);

            // Start the storyboard on the img control.
            sb.Begin(img);
        }

        /// <summary>
        /// Start download. When it finishes, start unzipping.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DownloadAndInstall(object sender, RoutedEventArgs e)
        {
            EnableGUI(false);
            Downloader downloader = new WebDownloader(this, (await updateHandler.GetLatestRelease()).Link);
            await downloader.Download();
            await Task.Run(() => { Unzip(Directory.GetCurrentDirectory() + "\\" + FILE_NAME + ".zip"); });
            //delete the zip after downloading
            if (File.Exists(Directory.GetCurrentDirectory() + "\\" + FILE_NAME + ".zip"))
            {
                File.Delete(Directory.GetCurrentDirectory() + "\\" + FILE_NAME + ".zip");
            }
        }

        private void EnableGUI(bool enabled)
        {
            button.IsEnabled = enabled;
            recoveryButton.IsEnabled = enabled;
        }

        /// <summary>
        /// Unzip a file and show progress in <see cref="BarGrid"/>.
        /// </summary>
        /// <param name="zipPath">Archive to uzip's full path.</param>
        private async void Unzip(string zipPath)
        {
            try
            {
                string extractedDirectory = zipPath.Replace(".zip", "");
                using (ZipFile zip = ZipFile.Read(zipPath))
                {
                    int totalFiles = zip.Count;
                    int filesExtracted = 0;
                    foreach (ZipEntry d in zip)
                    {
                        Debug.WriteLine(d.FileName);
                        d.Extract(extractedDirectory);
                        filesExtracted++;
                        float progress = (float)filesExtracted / totalFiles * 100;
                        this.Dispatcher.Invoke(() =>
                        {
                            this.infoLabel.Content = String.Format(Properties.Langs.Lang.installation_ip, progress);
                            UpdateBarProgress(progress);
                        });
                    }
                }
                Properties.Settings.Default.Version = (await updateHandler.GetLatestRelease()).Tag;
                Properties.Settings.Default.CurrentCommit = (await updateHandler.GetLatestRelease()).Commit;
                Properties.Settings.Default.Save();
                this.Dispatcher.Invoke(() =>
                {
                    InitPlayButton();

                });
            }
            catch (Exception ex)
            {
                this.infoLabel.Content = String.Format(Properties.Langs.Lang.installation_error, ex.Message);
                this.button.IsEnabled = false;
                this.recoveryButton.IsEnabled = true;
                UpdateBarProgress(0);
                PhpManager.ReportError(String.Format("Error in MainWindow.Unzip({0}): {1}", zipPath, ex.Message));
            }
        }



        /// <summary>
        /// Search for program executable and launch it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchGame(object sender, RoutedEventArgs e)
        {
            var executable = Properties.Resources.Executable;
            Process process = new Process();
            foreach (string f in Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "\\" + FILE_NAME, executable, SearchOption.AllDirectories))
            {
                process.StartInfo.FileName = f;
                process.Start();
                PhpManager.UpdateVersion(Properties.Settings.Default.Version);
                CloseWindow(sender, e);
                return;
            }
            PhpManager.ReportError(String.Format("{0} not found in {1}", executable, Directory.GetCurrentDirectory() + "\\" + FILE_NAME));
            MessageBox.Show(String.Format(Properties.Langs.Lang.exe_not_found, executable), Properties.Langs.Lang.warning);

        }

        private void InitSaveButtons()
        {
            Button[] buttons = new Button[] { save1Button, save2Button, save3Button, save4Button, save5Button, save6Button };
            buttons[Properties.Settings.Default.SaveSelected].Background = (Brush)new BrushConverter().ConvertFrom(Properties.Resources.ButtonSelectedBG);
            buttons[Properties.Settings.Default.SaveSelected].BorderBrush = (Brush)new BrushConverter().ConvertFrom(Properties.Resources.ButtonSelectedBorder);

        }

        //TODO: test
        private void ChangeSave(object sender, RoutedEventArgs e)
        {
            Button[] buttons = new Button[] { save1Button, save2Button, save3Button, save4Button, save5Button, save6Button };
            int newSlot = Array.IndexOf(buttons, sender);
            int oldSlot = Properties.Settings.Default.SaveSelected;
            if (newSlot == oldSlot) return;
            ((Button)sender).Background = (Brush)new BrushConverter().ConvertFrom(Properties.Resources.ButtonSelectedBG);
            ((Button)sender).BorderBrush = (Brush)new BrushConverter().ConvertFrom(Properties.Resources.ButtonSelectedBorder);
            buttons[oldSlot].Background = (Brush)new BrushConverter().ConvertFrom(Properties.Resources.ButtonBG);
            buttons[oldSlot].BorderBrush = (Brush)new BrushConverter().ConvertFrom(Properties.Resources.ButtonBorder);
            Properties.Settings.Default.SaveSelected = newSlot;
            Properties.Settings.Default.Save();
            string savePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Saved Games\" + Properties.Resources.SaveDirectory;
            string backupPath = savePath + String.Format("\\Save{0}", oldSlot);
            DirectoryInfo dirInfo = new DirectoryInfo(backupPath);
            if (dirInfo.Exists == false)
            {
                Directory.CreateDirectory(backupPath);
            }
            List<string> saveFiles = Directory.GetFiles(savePath).ToList();
            foreach (string file in saveFiles)
            {
                FileInfo mFile = new FileInfo(file);
                if (new FileInfo(backupPath + "\\" + mFile.Name).Exists == true)
                {
                    File.Delete(backupPath + "\\" + mFile.Name);
                }
                mFile.MoveTo(backupPath + "\\" + mFile.Name);
            }
            string newSavePath = savePath + String.Format("\\Save{0}", newSlot);
            dirInfo = new DirectoryInfo(newSavePath);
            if (dirInfo.Exists == false)
            {
                Directory.CreateDirectory(newSavePath);
            }
            saveFiles = Directory.GetFiles(newSavePath).ToList();
            foreach (string file in saveFiles)
            {
                FileInfo mFile = new FileInfo(file);
                if (new FileInfo(savePath + "\\" + mFile.Name).Exists == true)
                {
                    File.Delete(savePath + "\\" + mFile.Name);
                }
                mFile.MoveTo(savePath + "\\" + mFile.Name);
            }

        }

        /// <summary>
        /// Enable <see cref="button"/>, change its color and content and set <see cref="LaunchGame(object, RoutedEventArgs)"/> as function to be called on click.
        /// </summary>
        private void InitPlayButton()
        {
            EnableGUI(true);
            RemoveRoutedEventHandlers(this.button, Button.ClickEvent);
            this.button.Click += LaunchGame;
            GradientStopCollection c = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(255, 88, 139, 64), 0),
                new GradientStop(Color.FromArgb(255, 130, 183, 104), 0.3),
                new GradientStop(Color.FromArgb(255, 130, 183, 104), 0.7),
                new GradientStop(Color.FromArgb(255, 88, 139, 64), 1.0)
            };
            LinearGradientBrush b = new LinearGradientBrush(c)
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            this.button.Background = b;
            this.button.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#4e7137");
            this.button.Content = Properties.Langs.Lang.play;
            this.BarGrid.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// Make top Rectangle draggable
        /// </summary>
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Close the window and display any catched exception message in a popup window.
        /// </summary>
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                //Display exception in popup window. 
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Show/hide options windows. In addition hide notice window. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToggleOptions(object sender, RoutedEventArgs e)
        {
            inNotice = false;
            noticeWindow.Visibility = Visibility.Hidden;
            inOptions = !inOptions;
            if (inOptions)
            {
                optionWindow.Visibility = Visibility.Visible;
            }
            else
            {
                optionWindow.Visibility = Visibility.Hidden;
            }
        }

        private void ToggleNotice(object sender, RoutedEventArgs e)
        {
            inNotice = !inNotice;
            if (inNotice)
            {
                noticeWindow.Visibility = Visibility.Visible;
            }
            else
            {
                noticeWindow.Visibility = Visibility.Hidden;
            }
        }

        private async void Recover(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = System.Windows.MessageBox.Show(Properties.Langs.Lang.recovery_text, Properties.Langs.Lang.recovery, MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                inOptions = false;
                inNotice = false;
                noticeWindow.Visibility = Visibility.Hidden;
                optionWindow.Visibility = Visibility.Hidden;
                EnableGUI(false);
                string fullpath = Directory.GetCurrentDirectory() + "\\" + FILE_NAME;
                if (Directory.Exists(fullpath))
                {
                    label.Content = Properties.Langs.Lang.deleting;
                    SetBarIdle(true);
                    await Task.Factory.StartNew(path => Directory.Delete((string)path, true), Directory.GetCurrentDirectory() + "\\" + FILE_NAME);
                    SetBarIdle(false);
                }
                DownloadAndInstall(this, null);
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                return;
            }
        }

        /// <summary>
        /// Updates the progress bar to the given value.
        /// </summary>
        /// <param name="value">The target value.</param>
        public void UpdateBarProgress(double value)
        {
            foreach (var child in BarGrid.Children)
            {
                ((ProgressBar)child).Value = value;
            }
        }
        /// <summary>
        /// Set ProgressBar.IsIndeterminate = idle
        /// </summary>
        /// <param name="idle">true for show indeterminate loading, false otherwise</param>
        public void SetBarIdle(bool idle)
        {
            foreach (var child in BarGrid.Children)
            {
                ((ProgressBar)child).IsIndeterminate = idle;
            }
        }

        private bool IsGameInstalled()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\" + FILE_NAME);
            if (dirInfo == null) return false;
            return dirInfo.Exists && (dirInfo.GetDirectories().Length > 0 || dirInfo.GetFiles().Length > 0);
        }

        /// <summary>
        /// Removes all event handlers subscribed to the specified routed event from the specified element.
        /// Credit: https://stackoverflow.com/a/16392387/1149773
        /// </summary>
        /// <param name="element">The UI element on which the routed event is defined.</param>
        /// <param name="routedEvent">The routed event for which to remove the event handlers.</param>
        private static void RemoveRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

            // If no event handlers are subscribed, eventHandlersStore will be null.
            if (eventHandlersStore == null)
                return;

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
                "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
                eventHandlersStore, new object[] { routedEvent });

            // Iteratively remove all routed event handlers from the element.
            foreach (var routedEventHandler in routedEventHandlers)
                element.RemoveHandler(routedEvent, routedEventHandler.Handler);
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://docs.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
