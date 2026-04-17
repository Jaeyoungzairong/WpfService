using DevExpress.Mvvm;
using DevExpress.Xpf.Dialogs;
using WpfService.Models;
using WpfService.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfService.ViewModels
{
    public class RecoveryViewModel : ViewModelBase, IDisposable
    {
        private readonly ShellViewModel _shell;
        private readonly StringBuilder _log = new();

        public bool IsRunning { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsDebug 
        { 
            get => GetValue<bool>();
            set
            {
                if (SetValue(value))
                    Recovery.Instance.IsDebug = value;
            }
        }

        public List<string> PortItem { get => GetValue<List<string>>(); set => SetValue(value); }

        public string SelectedPort { get => GetValue<string>(); set => SetValue(value); }

        public string LogText { get => GetValue<string>(); set => SetValue(value); }
        
        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public string LoadingText { get => GetValue<string>(); set => SetValue(value); }

        public DelegateCommand LoadCommand => new(OnLoad);
        public AsyncCommand RecoveryCommand => new(StartAsync);
        public DelegateCommand SettingCommand => new(OpenRecoveryTool);
        public DelegateCommand BackCommand => new(OnBackButton_Click);
        public DelegateCommand RefreshCommand => new(OnPopup_Opening);

        public RecoveryViewModel(ShellViewModel shell)
        {
            _shell = shell;
            LoadingText = "Recovery is in progress...";
        }

        public void OnLoad()
        {
            ClearLog();
            WriteLog("Select a port and press START to enter recovery mode.");
            Recovery.Instance.ResponseReceived += ResponseReceived;
        }

        public void Dispose()
        {
            Recovery.Instance.Close();
            Recovery.Instance.ResponseReceived -= ResponseReceived;
        }

        private async Task StartAsync()
        {
            ClearLog();
            if (string.IsNullOrEmpty(SelectedPort))
            {
                Dialog.Warning("No serial port selected.");
                return;
            }
            else if (!Recovery.Instance.IsRKDevToolExists)
            {
                Dialog.Warning("The recovery program was not found.");
                return;
            }

            IsRunning = true;
            bool ret = Recovery.Instance.Open(SelectedPort);
            if (!ret)
            {
                IsRunning = false;
                return;
            }

            ret = await Recovery.Instance.StartRecoveryAsync();
            if (!ret)
            {
                IsRunning = false;
                return;
            }

            var proc = Recovery.Instance.OpenRKDevTool();
            if (proc != null)
            {
                IsLoading = true;
                await Task.Run(() => proc.WaitForExit());
                IsLoading = false;
            }

            //await OpenRecoveryToolAsnyc();
            IsRunning = false;
        }

        private void OpenRecoveryTool()
        {
            string initialDirectory = Recovery.Instance.IsRKDevToolExists  ? Path.GetDirectoryName(Config.Instance.RecoveryFileName) : @"C:\";
            var dlg = new DXOpenFileDialog
            {
                InitialDirectory = initialDirectory,
                Filter = "Executable Files (*.exe)|*.exe",
                DefaultExt = ".exe",
                AddExtension = true,
                CheckFileExists = true,
                Multiselect = false
            };

            if (dlg.ShowDialog() == true)
            {
                Config.Instance.RecoveryFileName = dlg.FileName;
                Config.Instance.WriteXml();
            }
        }

        private void WriteLog(string text)
        {
            _log.AppendLine(text);
            LogText = _log.ToString();
        }

        private void ClearLog()
        {
            _log.Clear();
            LogText = string.Empty;
        }

        private void ResponseReceived(string header, string content)
        {
            WriteLog($"[{header}] {content}");
        }

        private void OnPopup_Opening()
        {
            PortItem = SerialPort.GetPortNames().ToList();
            if (PortItem.Count < 1 || !PortItem.Contains(SelectedPort))
                SelectedPort = null;
        }

        private void OnBackButton_Click()
        {
            if (IsRunning)
            {
                var result = Dialog.Warning("Recovery is in progress. Move to the login page?", button: MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                    return;
            }

            IsRunning = false;
            IsLoading = false;
            _shell.NavigateToLogin();
        }
    }
}
