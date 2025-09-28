# yt-plus

Download any video directly from your browser without having to rely on 3rd party services.

**yt-plus** is a locally hosted video downloading server that uses a self-updating instance of [yt-dlp](https://github.com/yt-dlp/yt-dlp/) in the background.  
It comes with an extension where you can track and manage all of your downloads.

## How do I use it?
Simply click the `Download` button injected into your Youtube web player to download the video.  
Alternatively, you may also open the extension popup and press `Get This Video`, either works.
You can also set the 🍪 cookies or proxies in the `Settings` page inside the extension popup in case you get rate limited (or want to up your download quality if you have Youtube premium).

The downloader automatically gets the **best quality** available;  
All of the downloads will appear inside the 'Downloads' directory where the yt-plus binaries are located.

## Dependencies
Server is written in C#, therefore [.NET Runtime](https://dotnet.microsoft.com/en-us/download) is required to run it.  
You should also have `ffmpeg` installed to make sure yt-dlp works properly.

**Arch Linux**:  
```
sudo pacman -S dotnet-runtime ffmpeg
```

## Installation
- to be added

## Requirements to run
[Arch Linux](https://archlinux.org/) with [Zen](https://flathub.org/en/apps/app.zen_browser.zen) browser (Tested, works)  
Technically any Firefox-based browser on either Windows or Linux should work (Not tested)

> [!WARNING]
> If you're on Windows you may need to manually install [ffmpeg](https://github.com/yt-dlp/FFmpeg-Builds).

#### What about Chrome?
Chrome is currently <ins>unsupported</ins> as it fails to connect to the server, I'm assuming it's a mixed content issue.  
This would require the server to use `wss://` rather than the current `ws://` protocol

`popup.js` is already coded to support Chrome, but I don't have the time to fix said issue

Another thing is the hard-coded value for the popup's `border-radius` property within Chrome, which makes it look ugly
