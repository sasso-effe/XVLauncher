# XVLauncher
<img src="https://github.com/sasso-effe/XVLauncher/blob/main/Resources/splash.png?raw=true" width="400" height="300">


XVLauncher is an open-source launcher to handle download, installation and update process of Windows apps in a way as user-friendly as possibile.
As it has been desigend for RPG Maker projects, it is perfect to distribute games, but it is great for every kind of application.
It has been developed by <a href="https://github.com/sasso-effe">sasso-effe</a> and <a href="https://github.com/xZekro51">xZekro51</a>.

# Documentation
<table>
<tbody>
<tr>
<td><a href="https://github.com/sasso-effe/XVLauncher/wiki/Downloader">Downloader</a></td>
<td><a href="https://github.com/sasso-effe/XVLauncher/wiki/MainWindow">MainWindow</a></td>
</tr>
<tr>
<td><a href="https://github.com/sasso-effe/XVLauncher/wiki/MegaDownloader">MegaDownloader</a></td>
<td><a href="https://github.com/sasso-effe/XVLauncher/wiki/PhpManager">PhpManager</a></td>
</tr>

<tr>
<td><a href="https://github.com/sasso-effe/XVLauncher/wiki/SecurityManager">SecurityManager</a></td>
<td><a href="https://github.com/sasso-effe/XVLauncher/wiki/UpdateDownloader">UpdateDownloader</a></td>
</tr>
<tr>
<td><a href="https://github.com/sasso-effe/XVLauncher/wiki/UpdateHandler">UpdateHandler</a></td>
<td><a href="https://github.com/sasso-effe/XVLauncher/wiki/WebDownloader">WebDownloader</a></td>
</tr>
</tbody>
</table>

# Getting started (Work in progress)
To start costumizing XVLauncher for your app you just have to click on "Use this template" to create a new repository based on this one: your new repository will have all the files and directory contained in this one.

You can then clone your repository on Visual Studio (or any other solution you want to use to work in C# on a WPF application).

Having access to all the code, you can customize everything and create every feature you need, but we advise you to start choosing where do you want to host your application's files and how to download them. XVLauncher provides the Downloader abstract class which come with 3 implementations: MegaDownloader to download archives from mega.nz, WebDownloader to download files from direct urls, and UpdateDownloader to manage updates from GitLab repositories. You can use one of them or implement yours.

We will see how to host a directory on GitLab:
