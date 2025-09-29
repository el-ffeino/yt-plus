using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class Downloader
{
    public event EventHandler<string>? OnDownloadUpdate;
    public event EventHandler<string>? OnDestinationUpdate;
    public event EventHandler? OnDownloadCancel;
    private List<string> Files = new List<string>();

    public Downloader(string URL, int Download_ID)
    {
        var Downloader = new Process();

        string Arguments = $"-P {Downloads} {URL}";
        Downloader.StartInfo.FileName = ytdlp.Path;
        Downloader.StartInfo.Arguments = Arguments;
        Downloader.StartInfo.UseShellExecute = false;
        Downloader.StartInfo.RedirectStandardOutput = true;

        Downloader.OutputDataReceived += (sender, e) =>
        {
            if (Aborted_Downloads.Contains(Download_ID)) {
                if (!Downloader.HasExited)
                {
                    OnDownloadCancel?.Invoke(this, EventArgs.Empty);
                    Aborted_Downloads.Remove(Download_ID);
                    Downloader.Kill(true);

                    foreach (string file in Files) 
                    {
                        string FileName = File.Exists(file) ? file : $"{file}.part";
                        if (File.Exists(FileName)) {
                            File.Delete(FileName);
                        }
                    }
                }
                else Write($"Unable to kill downloader process `{Download_ID}` (exited)");
            }

            string? Output = e.Data;
            if (!String.IsNullOrEmpty(Output))
            {
                bool IsDestinationUpdate = Output.Contains("Destination") || Output.Contains("Merging formats into");
                if (IsDestinationUpdate) {
                    Match Destination = Regex.Match(Output, @"Destination:\s+(.*)|""([^""]*)""$");
                    string Download_Path = (Destination.Groups[1].Success ? Destination.Groups[1].Value : Destination.Groups[2].Value).Trim();
                    Files.Add(Download_Path);
                    OnDestinationUpdate?.Invoke(this, Download_Path);
                    return;
                }

                if (Output.Contains("Deleting original file")) {
                    Match File_Name = Regex.Match(Output, @"Deleting original file (.+?)(?: \[.+?\]\..+?)? \((pass -k to keep)\)");
                    if (File_Name.Success) {
                        string file = File_Name.Groups[1].Value.Trim();
                        if (Files.Contains(file)) {
                            Files.Remove(file);
                        }
                    }
                }

                Match Progress = Regex.Match(Output, @"\b(\d+(?:\.\d+)?)%\s");
                if (Progress.Success) {
                    OnDownloadUpdate?.Invoke(this, Progress.Value.Trim());
                }
            }
        };

        Task.Run(() =>
        {
            Downloader.Start();
            Downloader.BeginOutputReadLine();
            Downloader.WaitForExit();
            Downloader.Close();
        });
    }
}