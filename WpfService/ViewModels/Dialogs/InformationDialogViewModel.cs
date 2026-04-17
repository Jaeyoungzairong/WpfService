using DevExpress.Mvvm;
using WpfService.Models;
using WpfService.Services;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;


namespace WpfService.ViewModels.Dialogs
{
    public class InformationDialogViewModel : ViewModelBase
    {
        private string _oldSerialNumber;

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public string User { get => GetValue<string>(); set => SetValue(value); }

        public string Host { get => GetValue<string>(); set => SetValue(value); }

        public string SerialNumber { get => GetValue<string>(); set => SetValue(value); }

        public string VersionSW { get => GetValue<string>(); set => SetValue(value); }

        public string VersionFW { get => GetValue<string>(); set => SetValue(value); }

        public string SpectrometerSN { get => GetValue<string>(); set => SetValue(value); }

        public string SpectrometerPN { get => GetValue<string>(); set => SetValue(value); }

        public string Version { get => GetValue<string>(); set => SetValue(value); }

        public bool IsReadOnly => !Account.Instance.IsMaster;

        public bool IsSerialNumberChanged => !string.IsNullOrEmpty(SerialNumber) && SerialNumber != _oldSerialNumber;


        public AsyncCommand LoadCommand => new(LoadAsync);

        public InformationDialogViewModel() { }

        private async Task LoadAsync()
        {
            IsLoading = true;
            User = Account.Instance.User;
            Host = ApiClient.BaseUrl;
            Version = Assembly.GetEntryAssembly().GetName().Version.ToString();

            var response = await ApiClient.GetSeirialNumber();
            if (!response.Result)
            {
                IsLoading = false;
                return;
            }
            SerialNumber = response.Data.ToString();
            _oldSerialNumber = SerialNumber;

            response = await ApiClient.GetSwVersion();
            if (!response.Result)
            {
                IsLoading = false;
                return;
            }
            VersionSW = response.Data.ToString();

            response = await ApiClient.GetFwVersion();
            if (!response.Result)
            {
                IsLoading = false;
                return;
            }
            VersionFW = response.Data.ToString();

            response = await ApiClient.GetSpectrometerSN();
            if (!response.Result)
            {
                IsLoading = false;
                return;
            }
            SpectrometerSN = response.Data.ToString();

            //response = await ApiClient.GetSpectrometerPN();
            //if (!response.Result)
            //{
            //    IsLoading = false;
            //    return;
            //}
            //SpectrometerPN = response.Data.ToString();

            IsLoading = false;
        }
    }
}
