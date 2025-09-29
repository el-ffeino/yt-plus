using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ytplus;
public static class Globals
{
    public static string Server_Version = "initial release";
    public static int Server_Port = 10575;

    public static string Base_Directory = AppDomain.CurrentDomain.BaseDirectory;
    public static string Platform = RuntimeInformation.OSDescription;
    public static bool Linux = Platform.Contains("Linux");

    public static string Certificate_Path = Path.Combine(Base_Directory, "localhost.pfx");
    public static string Certificate_Pass = "chromesucks";

    public static string yt_dlp = ytdlp.GetExecutableName();
    public static string Downloads = Path.Combine(Base_Directory, "Downloads");
    public static string User_Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.3";
    public static string Config_Path = Base_Directory + "config.json";

    public static int Download_Index = 0;
    public static List<int> Aborted_Downloads = new List<int>();

    public class ClientMessage
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("payload")]
        public object? Payload { get; set; } = null; 
    }

    public class DownloadPayload
    {
        [JsonPropertyName("url")]
        public string URL {get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }

    public class BrowsePayload
    {
        [JsonPropertyName("path")]
        public required string Path { get; set; }
    }

    public class CancelPayload
    {
        [JsonPropertyName("download_id")]
        public required int Download_ID { get; set; }
    }
}