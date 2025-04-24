using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;

namespace AproTools
{
    public static class Updater
    {
        private const string owner = "palo49";
        private const string repo = "AproTools";
        private static string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static readonly string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
        private static readonly string tempPath = Path.Combine(Path.GetTempPath(), "AproToolsUpdate");

        public static Func<string, Task>? Logger;
        public static Action<int>? ReportProgress;

        private static readonly string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AproTools");
        private static readonly string checkTimestampPath = Path.Combine(appDataPath, "last_update_check.txt");

        public static async Task<(bool isAvailable, string? latestVersion)> CheckForUpdateInfoAsync(Func<string, Task>? log = null, bool force = false)
        {
            Directory.CreateDirectory(appDataPath);

            // Automatická kontrola jen 1x denně
            if (!force && File.Exists(checkTimestampPath))
            {
                var lastCheck = DateTime.Parse(File.ReadAllText(checkTimestampPath));
                if ((DateTime.Now - lastCheck).TotalHours < 24)
                {
                    if (log != null) await log("⏱ Aktualizace již byla dnes zkontrolována.");
                    return (false, null);
                }
            }

            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("AproToolsUpdater");

            try
            {
                var json = await http.GetStringAsync(apiUrl);

                // Detekce rate limitu
                if (json.Contains("API rate limit exceeded"))
                {
                    if (log != null) await log("❗ Limit GitHub API vyčerpán – zkus to později nebo přidej token.");
                    return (false, null);
                }

                var release = JsonDocument.Parse(json).RootElement;
                var latest = release.GetProperty("tag_name").GetString();
                var downloadUrl = release.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();

                File.WriteAllText(checkTimestampPath, DateTime.Now.ToString());

                if (IsNewerVersion(latest, currentVersion))
                {
                    if (log != null) await log($"⬇️ Nová verze {latest} dostupná.");
                    return (true, latest);
                }
                else
                {
                    if (log != null) await log("✅ Aplikace je aktuální.");
                    return (false, latest);
                }
            }
            catch (Exception ex)
            {
                if (log != null) await log("🚫 Updater selhal: " + ex.Message);
                return (false, null);
            }
        }


        private static bool IsNewerVersion(string latest, string current)
        {
            return new Version(latest.TrimStart('v')) > new Version(current);
        }

        public static async Task<(bool IsAvailable, string? LatestVersion)> IsUpdateAvailableAsync()
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("AproToolsUpdater");

            try
            {
                var json = await http.GetStringAsync(apiUrl);
                var release = JsonDocument.Parse(json).RootElement;
                var latest = release.GetProperty("tag_name").GetString();


                if (IsNewerVersion(latest, currentVersion))
                {
                    return (true, latest);
                }

                return (false, latest);
            }
            catch
            {
                return (false, null);
            }
        }

        public static async Task<bool> StartUpdateAsync(string latestVersion, Func<string, Task>? log = null, Action<int>? reportProgress = null)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("AproToolsUpdater");

            try
            {
                if (log != null) await log($"📦 Stahuji verzi {latestVersion}...");
                var json = await http.GetStringAsync(apiUrl);
                var release = JsonDocument.Parse(json).RootElement;
                var downloadUrl = release.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();

                Directory.CreateDirectory(tempPath);
                var zipPath = Path.Combine(tempPath, "update.zip");

                // Stahování ZIP
                reportProgress?.Invoke(10);
                await File.WriteAllBytesAsync(zipPath, await http.GetByteArrayAsync(downloadUrl));

                // Rozbalení
                reportProgress?.Invoke(50);
                ZipFile.ExtractToDirectory(zipPath, tempPath, true);

                // Zapsání aktuální cesty pro pomocníka
                var installPathFile = Path.Combine(tempPath, "install_path.txt");
                File.WriteAllText(installPathFile, AppDomain.CurrentDomain.BaseDirectory);

                // Spuštění pomocného updateru
                var helperPath = Path.Combine(tempPath, "AproTools_Updater.exe");

                reportProgress?.Invoke(100);
                if (log != null) await log("🚀 Spouštím aktualizátor...");

                Process.Start(new ProcessStartInfo
                {
                    FileName = helperPath,
                    WorkingDirectory = tempPath
                });

                Environment.Exit(0);
                return true;
            }
            catch (Exception ex)
            {
                if (log != null) await log("⚠️ Selhalo stahování nebo spuštění updatu: " + ex.Message);
                return false;
            }
        }


    }
}
