# yt-plus

Download any video directly from your browser without having to rely on 3rd party services.

**yt-plus** is a locally hosted video downloading server that uses a self-updating instance of [yt-dlp](https://github.com/yt-dlp/yt-dlp/) in the background.  
It comes with an extension where you can track and manage all of your downloads.



https://github.com/user-attachments/assets/21bd473d-afcd-42d7-9081-99675a092424



## ⁉️ How do I use it?
Simply click the `Download` button injected into your Youtube web player to download the video.  
Alternatively, you may also open the extension popup and press `Get This Video`, either works.

You can also set the 🍪 cookies or proxies in the `Settings` page inside the extension popup in case you get rate limited or want to up your download quality if you have Youtube premium.

> [!TIP]
> These are equivalent to yt-dlp's `proxy` and `--cookies-from-browser` arguments

The downloader automatically gets the **best quality** available;  
All of the downloads will appear inside the 'Downloads' directory where the yt-plus binaries are located.

## 📦 Dependencies
Server is written in C#, therefore [.NET Runtime](https://dotnet.microsoft.com/en-us/download) is required to run it.  
You should also have `ffmpeg` installed to make sure yt-dlp works properly.

**Arch Linux**:  
```console
sudo pacman -S dotnet-runtime ffmpeg
```

## 🛠 Installation
##### Server
- Get the latest [release](https://github.com/el-ffeino/yt-plus/releases) for either Linux or Windows
- Unpack it and run `yt-plus` executable

##### Extension
- Head over to `about:debugging#/runtime/this-firefox` in your Firefox
- Load temporary Add-on
- Select `manifest.json` from the `Extension` directory

#### How do I permanently install this?
Chrome version requires `wss://` to function and Firefox wants your name, address and soul in order to make an extension installable  
Feel free to post it on Firefox addons page yourself, I really could not be bothered

## Requirements to run
[Arch Linux](https://archlinux.org/) with [Zen](https://flathub.org/en/apps/app.zen_browser.zen) browser (Tested, works)  
Technically any Firefox-based browser on either Windows or Linux should work (Not tested)

> [!WARNING]
> If you're on Windows you may need to manually install [ffmpeg](https://github.com/yt-dlp/FFmpeg-Builds).

#### What about Chrome?
Chrome is currently <ins>unsupported</ins> as it fails to connect to the server, I'm assuming it's a mixed content issue.  
This would require the server to use `wss://` rather than the current `ws://` protocol

`popup.js` already supports Chrome, but I don't have the time to fix the issue mentioned above.

Another thing is the hard-coded value for the popup's `border-radius` property within Chrome, which makes it look ugly
