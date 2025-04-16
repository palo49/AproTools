using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace AproTools.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Opened += async (_, _) =>
            {
                await Updater.CheckForUpdateAsync();
            };
        }

        private void ThemeToggle_Checked(object? sender, RoutedEventArgs e)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        }

        private void ThemeToggle_Unchecked(object? sender, RoutedEventArgs e)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        }

    }
}