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
        private static readonly string tempPath = Path.Combine(Path.GetTempPath(), "MyAppUpdate");

        public static async Task CheckForUpdateAsync()
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("MyAppUpdater");

            try
            {
                var json = await http.GetStringAsync(apiUrl);
                var release = JsonDocument.Parse(json).RootElement;
                var latest = release.GetProperty("tag_name").GetString();
                var downloadUrl = release.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();

                if (IsNewerVersion(latest, currentVersion))
                {
                    Console.WriteLine($"Nová verze {latest} dostupná. Stahuji...");
                    Directory.CreateDirectory(tempPath);
                    var zipPath = Path.Combine(tempPath, "update.zip");
                    await File.WriteAllBytesAsync(zipPath, await http.GetByteArrayAsync(downloadUrl));
                    ZipFile.ExtractToDirectory(zipPath, tempPath, true);

                    // Uložíme cestu do dočasné složky
                    var installPathFile = Path.Combine(tempPath, "install_path.txt");
                    File.WriteAllText(installPathFile, AppDomain.CurrentDomain.BaseDirectory);

                    // Spuštění pomocníka
                    var helperPath = Path.Combine(tempPath, "UpdaterHelper.exe");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = helperPath,
                        WorkingDirectory = tempPath
                    });

                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Aplikace je aktuální.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Updater selhal: " + ex.Message);
            }
        }

        private static bool IsNewerVersion(string latest, string current)
        {
            return new Version(latest.TrimStart('v')) > new Version(current);
        }
    }
}
