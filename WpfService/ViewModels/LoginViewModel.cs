using DevExpress.Mvvm;
using Newtonsoft.Json.Linq;
using WpfService.Models;
using WpfService.Services;
using WpfService.ViewModels.Dialogs;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfService.ViewModels
{
    public class LoginViewModel : DialogViewModelBase
    {
        private readonly ShellViewModel _shell;
        private readonly NetworkDialogViewModel _settings = new();
        private bool _hasConnectionAttempted = false;

        public bool IsConnected { get => GetValue<bool>(); set => SetValue(value); }

        public ObservableCollection<string> UserList 
        { 
            get => GetValue<ObservableCollection<string>>();
            set => SetValue(value);
        }

        public string SelectedUser
        {
            get => GetValue<string>();
            set 
            { 
                if (SetValue(value))
                {
                    if (value == "master")
                        Password = "mdbest0802!";
                }
            }
        }

        public string Password { get => GetValue<string>(); set => SetValue(value); }

        public string Version => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public string LoadingText { get => GetValue<string>(); set => SetValue(value); }

        public AsyncCommand LoadCommand => new(LoadAsync);
        public AsyncCommand LoginCommand => new(LoginAsync);
        public AsyncCommand ConnectCommand => new(ConnectAsync);
        public DelegateCommand RecoveryCommand => new(ShowPasswordDialog);
        public DelegateCommand SettingCommand => new(ShowNetworkDialog);
        public DelegateCommand DebugCommand => new(OnLoginDebug);
        public DelegateCommand<KeyEventArgs> EnterCommand => new(OnLogin_KeyDown);
        

        public LoginViewModel(ShellViewModel shell)
        {
            _shell = shell;
            //LoadingText = "Waiting for response...";
        }

        private async Task LoadAsync()
        {
            Password = string.Empty;
            //if (!IsConnected && !_hasConnectionAttempted)
            //    await ConnectAsync();

            //_hasConnectionAttempted = true;
        }

        private async Task ConnectAsync()
        {
            ApiClient.IsDebug = false;
            IsLoading = true;
            UserList?.Clear();
            SelectedUser = null;
            var response = await ApiClient.WaitingForServerRespose(3);
            await Task.Delay(1000);
            IsConnected = response.Result;
            IsLoading = false;
            if (!IsConnected)
            {
                Dialog.Error(response.ErrMsg);
                return;
            }

            if (response.Data is JArray arr)
            {
                var list = arr.ToObject<ObservableCollection<string>>();
                list.Add("admin");
                list.Add("master");
                UserList = list;
                if ((UserList?.Count ?? 0) > 0)
                    SelectedUser = UserList.First();
            }
        }

        private async Task LoginAsync()
        {
            if (!ApiClient.IsConnected)
            {
                Dialog.Error("API is not connected.");
                return;
            }
            else if (string.IsNullOrEmpty(SelectedUser) || string.IsNullOrEmpty(Password))
            {
                Dialog.Warning("Please select ID and enter the password.");
                return;
            }

            var response = await ApiClient.Login(SelectedUser, Password);
            if (!response.Result)
            {
                Dialog.Error(response.ErrMsg);
                return;
            }

            Account.Instance.User = SelectedUser;
            ApiClient.IsDebug = false;
            _shell.NavigateToMain();
        }

        private void OnLoginDebug()
        {
            if (IsLoading)
                return;

            PasswordDialogViewModel password = new();
            var result = ShowDialog(password, "Password");
            if (result != MessageBoxResult.OK)
                return;

            if (password.Password == "mdbest0802!")
            {
                Account.Instance.User = "master";
                ApiClient.IsDebug = true;
                _shell.NavigateToMain();
            }
        }

        private void ShowPasswordDialog()
        {
            PasswordDialogViewModel password = new();
            var result = ShowDialog(password, "Password");
            if (result != MessageBoxResult.OK)
                return;

            if (password.Password == "mdbest0802!")
                _shell.NavigateToRecovery();
            else
                Dialog.Warning("Wrong password");
        }

        private void ShowNetworkDialog()
        {
            var result = ShowDialog(_settings, "Network");
            if (result != MessageBoxResult.OK)
                return;

            string host = _settings.HostAddress;
            string ip = _settings.LocalAddress;
            string netmask = _settings.Netmask;
            string gateway = _settings.Gateway;

            if (_settings.IsEthernet)
            {
                string ifName = _settings.SelectedNetworkName;
                var ethernet = NetworkManager.GetEthernetInterface(ifName);
                if (ethernet == null)
                {
                    Dialog.Error("Invalid ethernet interface.");
                    return;
                }

                int code = 0;
                if (_settings.IsStatic)
                {
                    if (!IsValidIP(host) || !IsValidIP(ip) || !IsValidIP(netmask) || !IsValidIP(gateway))
                    {
                        Dialog.Error("Invalid IP.");
                        return;
                    }
                    code =  NetworkManager.SetStaticIPv4(ifName, ip, netmask, gateway);
                }
                else
                {
                    code = NetworkManager.SetDhcpIPv4(ifName);
                }

                if (code != 0)
                    Dialog.Error($"ErrorCode : {code}");
            }
            else
            {
                if (!IsValidIP(host))
                {
                    Dialog.Error("Invalid IP.");
                    return;
                }
            }

            Config.Instance.HostAddress = host;
            Config.Instance.WriteXml();
        }

        private bool IsValidIP(string val)
        {
            return IPAddress.TryParse(val, out var addr) &&
                addr.AddressFamily == AddressFamily.InterNetwork &&
                val.Split('.').Length == 4;
        }

        private void OnLogin_KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _ = LoginAsync();
        }
    }
}
