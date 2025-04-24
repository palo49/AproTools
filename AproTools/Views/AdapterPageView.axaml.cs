using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AproTools.Views;

public partial class AdapterPageView : UserControl
{
    public AdapterPageView()
    {
        InitializeComponent();
        LoadAdaptersAsync();
    }

    private async void LoadAdaptersAsync()
    {
        var adapters = await Task.Run(() =>
        {
            var list = new List<string>();
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ipProps = adapter.GetIPProperties();
                foreach (var unicast in ipProps.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        list.Add(adapter.Name);
                        break;
                    }
                }
            }
            return list.Distinct().ToList();
        });

        foreach (var adapter in adapters)
        {
            cmbAdapters.Items.Add(adapter);
        }


        if (!string.IsNullOrEmpty(AproTools.Default.Adapter_SelectedIndex))
        {
            cmbAdapters.SelectedItem = AproTools.Default.Adapter_SelectedIndex;
            UpdateInfo(AproTools.Default.Adapter_SelectedIndex);
        }

        txtboxIPv4.Text = AproTools.Default.Adapter_IPv4;
        txtboxSubnet.Text = AproTools.Default.Adapter_Subnet;
        txtboxGateway.Text = AproTools.Default.Adapter_Gateway;
    }

    private void UpdateInfo(string adapterName)
    {
        var adapter = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault(a => a.Name == adapterName);

        if (adapter is null)
            return;

        var ipProps = adapter.GetIPProperties();

        var unicast = ipProps.UnicastAddresses
            .FirstOrDefault(u => u.Address.AddressFamily == AddressFamily.InterNetwork);

        if (unicast is not null)
        {
            txtIPv4.Text = unicast.Address.ToString();
            txtSubnet.Text = unicast.IPv4Mask?.ToString() ?? string.Empty;
        }

        var gateway = ipProps.GatewayAddresses
            .FirstOrDefault()?.Address.ToString();

        txtGateway.Text = gateway ?? string.Empty;
    }

    private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (cmbAdapters.SelectedItem is string selectedName)
        {
            UpdateInfo(selectedName);
        }
    }

    private void buttonStatic_Click(object? sender, RoutedEventArgs e)
    {
        if (cmbAdapters.SelectedItem is string selectedAdapter)
            AproTools.Default.Adapter_SelectedIndex = selectedAdapter;

        AproTools.Default.Adapter_IPv4 = txtboxIPv4.Text;
        AproTools.Default.Adapter_Subnet = txtboxSubnet.Text;
        AproTools.Default.Adapter_Gateway = txtboxGateway.Text;

        AproTools.Default.Save();
    }
}
