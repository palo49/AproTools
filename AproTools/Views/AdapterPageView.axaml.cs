using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AproTools.Views;

public partial class AdapterPageView : UserControl
{
    public AdapterPageView()
    {
        InitializeComponent();
    }

    private void buttonStatic_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        buttonStatic.Content = "test";
    }
}