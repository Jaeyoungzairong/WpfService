using DevExpress.Mvvm;
using WpfService.Models;
using WpfService.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WpfService.ViewModels.Dialogs
{
    class NetworkDialogViewModel : ViewModelBase
    {
        public string HostAddress { get => GetValue<string>(); set => SetValue(value); }

        public int Port { get => GetValue<int>(); set => SetValue(value); }

        public List<string> NetworkNames { get => GetValue<List<string>>(); set => SetValue(value); }

        public string SelectedNetworkName 
        { 
            get => GetValue<string>(); 
            set
            {
                if (SetValue(value))
                {
                    LoadIp4Properties();
                    RaisePropertyChanged(nameof(IsEthernet));
                    RaisePropertyChanged(nameof(IsStatic));
                }
            }
        }

        public List<string> AddressModes { get; } = new List<string>{ "DHCP", "Static" };

        public string SelectedAddressMode 
        { 
            get => GetValue<string>();
            set
            {
                if (SetValue(value))
                    RaisePropertyChanged(nameof(IsStatic));
            }
        }

        public string LocalAddress { get => GetValue<string>(); set => SetValue(value); }

        public string Netmask { get => GetValue<string>(); set => SetValue(value); }

        public string Gateway { get => GetValue<string>(); set => SetValue(value); }

        public bool IsEthernet => SelectedNetworkName != "Wi-Fi";

        public bool IsStatic => IsEthernet && (SelectedAddressMode == "Static");

        public DelegateCommand LoadCommand => new(OnLoaded);

        public NetworkDialogViewModel() { }

        private void OnLoaded()
        {
            List<NetworkInterface> networkInterfaces = new();
            HostAddress = Config.Instance.HostAddress;
            Port = Config.Instance.Port;

            //var wifis = NetworkManager.GetWifiInterfaces();
            //if (wifis != null && wifis.Count > 0)
            //    networkInterfaces.AddRange(wifis);

            var ethernets = NetworkManager.GetEthernetInterfaces();
            if (ethernets != null && ethernets.Count > 0)
                networkInterfaces.AddRange(ethernets);

            if (networkInterfaces.Count == 0)
            {
                NetworkNames = null;
                SelectedNetworkName = null;
                LocalAddress = null;
                Netmask = null;
                Gateway = null;
                return;
            }

            NetworkNames = networkInterfaces.Select(n => n.Name).ToList();
            SelectedNetworkName = NetworkNames.First();
            if (IsEthernet)
            {
                var ethernet = ethernets.FirstOrDefault();

                var properties = ethernet.GetIPProperties();
                var ipv4Unicast = properties.UnicastAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .ToList();

                var ipList = ipv4Unicast.Select(a => a.Address.ToString()).ToArray();
                var maskList = ipv4Unicast.Select(i => i.IPv4Mask.ToString()).ToArray();
                var gwList = properties.GatewayAddresses
                    .Where(g => g.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(g => g.Address.ToString())
                    .ToArray();

                LocalAddress = string.Join(".", ipList);
                Netmask = string.Join(".", maskList);
                Gateway = string.Join(".", gwList);

            }
            else
            {
                LocalAddress = null;
                Netmask = null;
                Gateway = null;
            }
        }

        private void LoadIp4Properties()
        {
            var ethernet = NetworkManager.GetEthernetInterface(SelectedNetworkName);
            if (ethernet == null)
            {
                LocalAddress = null;
                Netmask = null;
                Gateway = null;
                return;
            }

            var properties = ethernet.GetIPProperties();
            var ipv4Props = properties.GetIPv4Properties();

            var ipv4Unicast = properties.UnicastAddresses
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .ToList();

            var ipList = ipv4Unicast.Select(a => a.Address.ToString()).ToArray();
            var maskList = ipv4Unicast.Select(i => i.IPv4Mask.ToString()).ToArray();
            var gwList = properties.GatewayAddresses
                .Where(g => g.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(g => g.Address.ToString())
                .ToArray();

            bool isDHCP = ipv4Props?.IsDhcpEnabled ?? false;
            SelectedAddressMode = isDHCP ? "DHCP" : "Static";

            SelectedNetworkName = ethernet.Name;
            LocalAddress = string.Join(".", ipList);
            Netmask = string.Join(".", maskList);
            Gateway = string.Join(".", gwList);
        }
    }
}
