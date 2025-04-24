using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AproTools.Views;

public partial class HomePageView : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _currentTime;
    public string CurrentTime
    {
        get => _currentTime;
        set
        {
            _currentTime = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
        }
    }

    public HomePageView()
    {
        InitializeComponent();

        DataContext = this;

        DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += (_, _) => CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        timer.Start();

        if (DesignHelper.IsInDesign)
            return;


        versionTxt.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

        this.Loaded += async (_, _) =>
        {
            Updater.Logger = Log;

            await Log("Kontroluji dostupnost nové verze...");
            var (isAvailable, latest) = await Updater.IsUpdateAvailableAsync();

            if (isAvailable)
            {
                await Log($"Nová verze {latest} je dostupná!");
                UpdateButton.IsEnabled = true;
            }
            else
            {
                if (latest == null)
                {
                    await Log("❗ Limit GitHub API vyčerpán – zkus to později nebo přidej token.");
                }
                else
                {
                    await Log("Vše šlape jako po másle – používáš nejnovější verzi." + latest);
                }
                UpdateButton.IsEnabled = false;
            }
        };

        UpdateButton.Click += async (_, _) =>
        {
            UpdateProgress.IsVisible = true;
            UpdateProgress.Value = 0; // začátek

            await Log("Spouštím aktualizaci...");

            var (available, version) = await Updater.CheckForUpdateInfoAsync(Log, force: true);
            if (available && version != null)
            {
                await Updater.StartUpdateAsync(version, Log, val => UpdateProgress.Value = val);
            }
        };
    }

    private async void ManualUpdateCheck_Click(object? sender, RoutedEventArgs e)
    {
        LogBox.Text += $"[{DateTime.Now:HH:mm:ss}] 🕵️‍♂️ Ruční kontrola aktualizace...\n";

        var (isAvailable, latest) = await Updater.CheckForUpdateInfoAsync(Log, force: true);

        if (isAvailable)
        {
            LogBox.Text += $"[{DateTime.Now:HH:mm:ss}] 🔔 Aktualizace dostupná: {latest}\n";
            UpdateButton.IsVisible = true;
        }
        else
        {
            LogBox.Text += $"[{DateTime.Now:HH:mm:ss}] ✅ Není potřeba aktualizace.\n";
        }
    }

    private async Task Log(string message)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            LogBox.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            LogBox.CaretIndex = LogBox.Text.Length;
        });
    }
}