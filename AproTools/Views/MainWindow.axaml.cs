using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AproTools.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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