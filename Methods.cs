using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json.Serialization;

namespace ytplus;
public static class Methods
{
    public static void Write(string str) => Console.WriteLine(str);

    public class Settings
    {
        [JsonPropertyName("Cookies")]
        public string Cookies { get; set; } = string.Empty;

        [JsonPropertyName("Proxy")]
        public string Proxy { get; set; } = string.Empty;

        public static string Path = System.IO.Path.Combine(Base_Directory, "config.json");

        public Settings()
        {
            if (!File.Exists(Path)) {
                Save();
            }
        }

        public void Save()
        {
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, jsonOptions);
            File.WriteAllText(Path, json);
        }

        public static Settings Load()
        {
            if (File.Exists(Path))
            {
                string json = File.ReadAllText(Path);
                var settings = JsonSerializer.Deserialize<Settings>(json);
                return settings ?? new Settings();
            }
            return new Settings();
        }
    }

    public class Item
    {
        public int? ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Progress { get; set; } = "0%";
        public string Destination { get; set; } = string.Empty;

        public Item(int id, string title, string source)
        {
            ID = id;
            Title = title;
            Source = source;
        }
    }

    public static class ytdlp
    {
        public static string Version { get; } = ytdlp.Run("--version");

        public static string Path => System.IO.Path.Combine(Base_Directory, yt_dlp);

        public static string GetExecutableName()
        {
            if (Linux) return "yt-dlp_linux";
            return "yt-dlp.exe";
        }

        public static string Run(string arguments)
        {
            arguments = arguments.Replace("\"", "\\\"");

            Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = ytdlp.Path;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            string ProcOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return ProcOutput.Trim();
        }

        public static async Task<bool> Installed()
        {
            if (File.Exists(ytdlp.Path)) return true;
            else 
            {
                string Github_API = "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest";
                Write($"{yt_dlp} not found in {Base_Directory}");
                Write($"Getting latest version from {Github_API}");

                using var client = new HttpClient { DefaultRequestHeaders = { { "User-Agent", User_Agent } } };
                try
                {
                    HttpResponseMessage response = await client.GetAsync(Github_API);
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    JsonElement root = doc.RootElement;

                    string? Download_URL = null;
                    foreach (JsonElement asset in root.GetProperty("assets").EnumerateArray())
                    {
                        if (asset.GetProperty("name").GetString() == yt_dlp)
                        {
                            Download_URL = asset.GetProperty("browser_download_url").GetString();
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(Download_URL)) {
                        throw new Exception($"Error getting {yt_dlp} from {Github_API} (not found)");
                    }

                    Write($"Getting {yt_dlp} from {Download_URL}");
                    byte[] ytdlp_dl = await client.GetByteArrayAsync(Download_URL);
                    await File.WriteAllBytesAsync(ytdlp.Path, ytdlp_dl);
                    Write($"Finished downloading latest yt-dlp version");

                    if (Linux) 
                    {
                        try
                        {
                            using var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "chmod",
                                    Arguments = $"+x \"{ytdlp.Path}\"",
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };

                            process.Start();
                            await process.WaitForExitAsync();
                            if (process.ExitCode == 0) {
                                Write($"Made {ytdlp.Path} executable");
                                return true;
                            } else {
                                string error = await process.StandardError.ReadToEndAsync();
                                Write($"Failed to make {yt_dlp} executable: {error}");
                                Write($"Try manually running `chmod +x {ytdlp.Path}`");
                            }
                        } catch (Exception ex) {
                            Write($"Error making {yt_dlp} executable: {ex.Message}");
                        }
                    } else {
                        return true;
                    }

                }
                catch (HttpRequestException ex)
                {
                    Write($"HTTP error while getting yt-dlp: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Write($"Error downloading yt-dlp: {ex.Message}");
                }
                Write($"[yt-plus] Failed to install {yt_dlp}, server exiting..");
                Environment.Exit(0);
                return false;
            }
        }

        public static void CheckForUpdates()
        {
            _ = ytdlp.IsUpdateAvailableAsync().ContinueWith(t => {
                if (t.Result) ytdlp.Update();
            });
        }

        public static async Task<bool> IsUpdateAvailableAsync()
        {
            using var client = new HttpClient { DefaultRequestHeaders = { { "User-Agent", User_Agent } } };
            try
            {
                string response = await client.GetStringAsync("https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest"); // Changed to await
                string? version = JsonDocument.Parse(response).RootElement.GetProperty("tag_name").GetString();
                
                if (version != null && 
                    int.TryParse(version.Replace(".", ""), out int Latest) && 
                    int.TryParse(Version.Replace(".", ""), out int Current))
                {
                    if (Latest > Current)
                    {
                        Write($"Newer version of yt-dlp ({version}) available for download");
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public static void Update()
        {
            Write($"Update for {yt_dlp} available, now installing..");
            string Arguments = "--update";

            if (!File.Exists(ytdlp.Path))
            {
                Write($"Error: {yt_dlp} not found at {ytdlp.Path}");
                return;
            }

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = ytdlp.Path;
                p.StartInfo.Arguments = Arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                p.Start();
                p.WaitForExit();

                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(output)) Write("yt-dlp output: " + output);
                if (!string.IsNullOrEmpty(error)) Write("yt-dlp error: " + error);

                if (p.ExitCode != 0) Write($"Update failed with exit code: {p.ExitCode}");
                else Write("Finished updating yt-dlp to the newest version.");
            }
            catch (Exception ex)
            {
                Write($"Failed to start update yt-dlp: {ex.Message}");
            }
        }
    }
}