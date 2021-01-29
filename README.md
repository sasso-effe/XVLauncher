# XVLauncher

XVLauncher is an open-source launcher to handle download, installation and update process of Windows apps in a way as user-friendly as possibile.
As it has been desigend for RPG Maker projects, it is perfect to distribute games, but it is great for every kind of application.
It has been developed by <a href="https://github.com/sasso-effe">sasso-effe</a> and <a href="https://github.com/xZekro51">xZekro51</a>.

# Documentation

<table>
<tbody>
<tr>
<td><a href="#downloader">Downloader</a></td>
<td><a href="#mainwindow">MainWindow</a></td>
</tr>
<tr>
<td><a href="#megadownloader">MegaDownloader</a></td>
<td><a href="#phpmanager">PhpManager</a></td>
</tr>

<tr>
<td><a href="#securitymanager">SecurityManager</a></td>
<td><a href="#updatedownloader">UpdateDownloader</a></td>
</tr>
<tr>
<td><a href="#updatehandler">UpdateHandler</a></td>
<td><a href="#webdownloader">WebDownloader</a></td>
</tr>
</tbody>
</table>


## Downloader

Abstract class that provides the template method Download(). It is implemented by <a href="#webdownloader">WebDownloader</a> and <a href="#megadownloader">MegaDownloader</a>.

### Constructor(window, url)

Constructor method.

| Name | Description |
| ---- | ----------- |
| window | *XVLauncher.MainWindow*<br><a href="#mainwindow">MainWindow</a> instance where are placed GUI elements to be updated to show progress. |
| url | *System.String*<br> |

### Download

Template method that only handle exceptions and do some simples GUI changes. Real Download implementation is delegated to sublasses that must implement the abstract method <a href="#downloader.downloadimplementation">Downloader.DownloadImplementation</a>.



### DownloadImplementation

Abstract method to be implemented by subclasses, that must provide download implementation, possibly asynchronously.  It is called by the template method <a href="#downloader.download">Downloader.Download</a>





## MainWindow

Interaction logic for MainWindow.xaml

### Constructor

This method is called when the program starts. It checks if the user has an ID assigned, if not it try to get a new ID from the database and assign it to the user. Then check if there are fonts to be installed and, if it is the case, installs them. Finally, initialise the GUI.

### CloseWindow(System.Object,System.Windows.RoutedEventArgs)

Close the window and display any catched exception message in a popup window.

### DisplayNextImage(System.Windows.Controls.Image)

Animation code for the image changing

### DownloadAndInstall(sender, e)

Start download. When it finishes, start unzipping.

| Name | Description |
| ---- | ----------- |
| sender | *System.Object*<br> |
| e | *System.Windows.RoutedEventArgs*<br> |

### InitializeComponent

InitializeComponent

### InitPlayButton

Enable <a href="#mainwindow.button">MainWindow.button</a>, change its color and content and set <a href="#mainwindow.launchgame(system.object,system.windows.routedeventargs)">MainWindow.LaunchGame(System.Object,System.Windows.RoutedEventArgs)</a> as function to be called on click.

### LaunchGame(sender, e)

Search for program executable and launch it.

| Name | Description |
| ---- | ----------- |
| sender | *System.Object*<br> |
| e | *System.Windows.RoutedEventArgs*<br> |

### MoveWindow(System.Object,System.Windows.Input.MouseButtonEventArgs)

Make top Rectangle draggable

### RemoveRoutedEventHandlers(element, routedEvent)

Removes all event handlers subscribed to the specified routed event from the specified element. Credit: https://stackoverflow.com/a/16392387/1149773

| Name | Description |
| ---- | ----------- |
| element | *System.Windows.UIElement*<br>The UI element on which the routed event is defined. |
| routedEvent | *System.Windows.RoutedEvent*<br>The routed event for which to remove the event handlers. |

### SetBarIdle(idle)

Set ProgressBar.IsIndeterminate = idle

| Name | Description |
| ---- | ----------- |
| idle | *System.Boolean*<br>true for show indeterminate loading, false otherwise |

### Tick(System.Object,System.EventArgs)

Tick on the dispatchtimer called every 10 seconds. Used to update background Images.

### ToggleOptions(sender, e)

Show/hide options windows. In addition hide notice window.

| Name | Description |
| ---- | ----------- |
| sender | *System.Object*<br> |
| e | *System.Windows.RoutedEventArgs*<br> |

### Unzip(zipPath)

Unzip a file and show progress in <a href="#mainwindow.bargrid">MainWindow.BarGrid</a>.

| Name | Description |
| ---- | ----------- |
| zipPath | *System.String*<br>Archive to uzip's full path. |

### UpdateBarProgress(value)

Updates the progress bar to the given value.

| Name | Description |
| ---- | ----------- |
| value | *System.Double*<br>The target value. |

### Window_Loaded(sender, e)

It is called when the GUI is loaded. It implements the main logic of the program.

| Name | Description |
| ---- | ----------- |
| sender | *System.Object*<br> |
| e | *System.Windows.RoutedEventArgs*<br> |


## MegaDownloader

Subclass of <a href="#downloader">Downloader</a> that use <a href="#cg.web.megaapiclient.megaapiclient">CG.Web.MegaApiClient.MegaApiClient</a> to download a zipped version of the project from a mega.nz url.

### Constructor(window, url)

Constructor method.

| Name | Description |
| ---- | ----------- |
| window | *XVLauncher.MainWindow*<br><a href="#mainwindow">MainWindow</a> where are placed GUI elements to be updated to show progress. |
| url | *System.String*<br>mega.nz link to download a .zip including the .exe to be launched and all the files needed. |



## PhpManager

This class provides methods to dialog with php files. To implement the methods of this class you need a server to host php files and a database. Php files and MySQL query to create the appropriate table are provided in the GitHub repository.

### FirstContact

Try to contact server and, if the connection is successful, set url to mega archive in `megaurl` and the tag of the last version available in `lastVer`. This method should not be used if the game is stored in a GitLab repository, in that case version tag can be retrieved with <a href="#xvlauncher.updatehandler.getlatestrelease">XVLauncher.UpdateHandler.GetLatestRelease</a>, while `megaurl` is no more useful.

#### Returns

`true` if the conncection is successful, `false` otherwise.

### GetID(version)

Assign to user a new auto-increment ID.

| Name | Description |
| ---- | ----------- |
| version | *System.String*<br>current game version tag. |

#### Returns

ID.

### GetLastVer

Getter method for lastVer.

#### Returns

Last version available.

### GetMegaUrl

Getter method for <a href="#phpmanager.megaurl">PhpManager.megaUrl</a>.




### ReportError(error)

Store error message in last_error column. Also update error_date column with current timestamp.

| Name | Description |
| ---- | ----------- |
| error | *System.String*<br>Error message. |

### SendPost(url, postData, report)

Helper method provided by makim on this Stack Overflow thread: https://stackoverflow.com/questions/6960426/c-sharp-xml-documentation-website-link.

| Name | Description |
| ---- | ----------- |
| url | *System.String*<br> |
| postData | *System.String*<br> |
| report | *System.Boolean*<br>In case of Exception, don't call ReportError if it is `false`. Default is `true`, but in some cases it can be useful to set it to `false` to avoid loops. |

#### Returns

webpage content.

### UpdateVersion(newVer)

Update user current version in the database. Since in the database the column last_date is setted as ON UPDATE CURRENT_TIMESTAMP, this method can also be used to update that column. For example, it can be called whenever the game is launched, so the database will register the last time each id has launched the game. It can be useful to know if someone is still playing the game (using the application/using whatever this launcher is used to) after months or years.

| Name | Description |
| ---- | ----------- |
| newVer | *System.String*<br>User current version. |

### USE_PHP

Set it to true if you want to use php files to communicate with your server. Check the documntation for more details.


## SecurityManager

Singleton class to generate php files path, just to avoid to put it as plain text. It is a really basic security system, do not use it to protect sensible data.

### GetInstance

Singleton pattern's getter method.

#### Returns

Singleton instance of <a href="#securitymanager">SecurityManager</a>.


## UpdateDownloader

Subclass of <a href="#downloader">Downloader</a> that handle updates from GitLab repository.

### Constructor(window, url, oldPath, newPath, handler)

Constructor method.

| Name | Description |
| ---- | ----------- |
| window | *XVLauncher.MainWindow*<br><a href="#mainwindow">MainWindow</a> instance where are placed GUI elements to be updated to show progress. |
| url | *System.String*<br> |
| oldPath | *System.Collections.Generic.List{System.String}*<br>List of path that do not exists in the new update, because file have been mooved or deleted. |
| newPath | *System.Collections.Generic.List{System.String}*<br>List of new path |
| handler | *XVLauncher.UpdateHandler*<br>An instance of <a href="#updatehandler">UpdateHandler</a> |


## UpdateHandler

Class for handling updates with git-based repositories.

### Constructor(window)

Constructor of the UpdateHandler class.

| Name | Description |
| ---- | ----------- |
| window | *XVLauncher.MainWindow*<br>The window where the updater will be called. |

### CheckUpdateAvailability

Check if it there is a new release on GitLab repo.

#### Returns

true if there is an update available, false otherwise.

### Compare(targetCommit)

Compares the currently stored commit id with a target commit id and returns a tuple of list with the old paths and new paths of changed files.

| Name | Description |
| ---- | ----------- |
| targetCommit | *System.String*<br>The target commit id using for comparing. |

#### Returns



### GetLatestRelease

Retrieves the latest github release informations.

#### Returns

last release download link, last commit name, last release tag.

### Window

The instance of MainWindow that contains GUI elements which show update progress and status.


## WebDownloader

Subclass of <a href="#downloader">Downloader</a> that use <a href="#system.net.webclient">System.Net.WebClient</a> to download a zipped version of the project from a direct download url.

### Constructor(window, url)

Constructor method.

| Name | Description |
| ---- | ----------- |
| window | *XVLauncher.MainWindow*<br><a href="#mainwindow">MainWindow</a> where are placed GUI elements to be updated to show progress. |
| url | *System.String*<br>direct link to download a .zip including the .exe to be launched and all the files needed. |
