global using static ytplus.Globals;
global using static ytplus.Methods;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;

if (await ytdlp.Installed()) {
    Write($"[yt-plus] Running {Server_Version} (yt-dlp {ytdlp.Version}) on {Platform}");
}

ytdlp.CheckForUpdates();
var Server = new WebSocketServer();
var Config = Settings.Load();
List<Item> Items = new List<Item>();

Server.OnMessageReceived += async (sender, message) =>
{
    var JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    ClientMessage? Message = JsonSerializer.Deserialize<ClientMessage>(message, JsonOptions);
    if (Message == null) return;

    switch (Message.Action)
    {
        case "Download":
        {
            var Payload = JsonSerializer.Deserialize<DownloadPayload>(Message.Payload?.ToString() ?? "{}", JsonOptions)!;
            int Download_ID = Download_Index++;

            await Server.Send(new { Action = "AddDownload", Payload = new { Index = Download_ID, Title = Payload.Title, Source = Payload.Source } });

            Downloader Download = new Downloader(Payload.URL, Download_ID);
            Item i = new Item(Download_ID, Payload.Title, Payload.Source);
            Items.Add(i);

            Download.OnDownloadUpdate += async (sender, str) => {
                i.Progress = str;
                var msg = new { Action = "DownloadUpdate", Payload = new { Download_ID = Download_ID, Progress = str } };
                await Server.Send(msg);
            };
            Download.OnDestinationUpdate += async (sender, str) => {
                i.Destination = str;
                var msg = new { Action = "DestinationUpdate", Payload = new { Download_ID = Download_ID, Destination = str } };
                await Server.Send(msg);
            };
            Download.OnDownloadCancel += async (sender, _) => {
                Items.Remove(i);
                await Task.CompletedTask;
            };
        }
        break;

        case "Cancel":
        {
            var Payload = JsonSerializer.Deserialize<CancelPayload>(Message.Payload?.ToString() ?? "{}", JsonOptions);
            if (Payload == null) return;

            try
            {
                if (!Aborted_Downloads.Contains(Payload.Download_ID))
                {
                    Aborted_Downloads.Add(Payload.Download_ID);
                }
            }
            catch (Exception ex)
            {
                Write($"Provided 'Download_ID' `{Payload.Download_ID}` is invalid: {ex.Message}");
            }
        }
        break;

        case "Finish":
        {
            var Payload = JsonSerializer.Deserialize<CancelPayload>(Message.Payload?.ToString() ?? "{}", JsonOptions);
            if (Payload == null) return;

            Item? i = Items.FirstOrDefault(item => item.ID == Payload.Download_ID);
            if (i != null) {
                Items.Remove(i);
            }
        }
        break;

        case "Fetch-Downloads":
        {
            var msg = new { Action = "Fetch-Downloads", Payload = new { Downloads = JsonSerializer.Serialize(Items) } };
            await Server.Send(msg);
        }
        break;

        case "Browse":
        {
            var Payload = JsonSerializer.Deserialize<BrowsePayload>(Message.Payload?.ToString() ?? "{}", JsonOptions)!;

            if (string.IsNullOrEmpty(Payload.Path)) return;

            if (File.Exists(Payload.Path))
            {
                string Handler = Linux ? "nautilus" : "explorer.exe";
                string Arguments = Linux ? $"--select \"{Payload.Path}\"" : $"/select,\"{Payload.Path}\"";
                if (Linux)
                {
                    try {
                        Process.Start(Handler, Arguments);
                    } catch {
                        Handler = "xdg-open";
                        Arguments = Path.GetDirectoryName(Payload.Path)!;
                        Process.Start(Handler, Arguments);
                    }
                }
                else Process.Start(Handler, Arguments);
            } else {
                Write($"File {Payload.Path} doesn't exist");
            }
        }
        break;

        case "Save":
        {
            var Payload = JsonSerializer.Deserialize<Settings>(Message.Payload?.ToString() ?? "{}", JsonOptions);
            if (Payload != null)
            {
                Config.Proxy = Payload.Proxy.Trim();
                Config.Cookies = Payload.Cookies.Trim();
                Config.Save();
            }
        }
        break;

        case "Load":
        {
            var msg = new { Action = "Load", Payload = new { Proxy = Config.Proxy, Cookies = Config.Cookies } };
            await Server.Send(msg);
        }
        break;

        default: Write($"Invalid message action received: `{Message.Action}`"); break;
    }

    await Task.CompletedTask;
};

await Server.Start("localhost", Server_Port);
Console.ReadKey();
Server.Stop();