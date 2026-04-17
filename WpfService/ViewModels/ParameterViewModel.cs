using DevExpress.Mvvm;
using WpfService.Models;
using WpfService.Services;
using System.Threading.Tasks;

namespace WpfService.ViewModels
{
    public partial class ParameterViewModel : ViewModelBase
    {
        private readonly ShellViewModel _shell;

        public object Common { get => GetValue<object>(); set => SetValue(value); }

        public object DNA { get => GetValue<object>(); set => SetValue(value); }

        public object UV { get => GetValue<object>(); set => SetValue(value); }

        public object Cuvette { get => GetValue<object>(); set => SetValue(value); }

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public AsyncCommand LoadCommand => new(LoadAsync);
        public AsyncCommand CommonCommand => new(SaveCommonAsync);
        public AsyncCommand DnaCommand => new(SaveDNAAsync);
        public AsyncCommand UvCommand => new(SaveUVAsync);
        public AsyncCommand CuvetteCommand => new(SaveCuvetteAsync);
        public DelegateCommand BackCommand => new(OnBackButton_Click);

        public ParameterViewModel(ShellViewModel shell)
        {
            _shell = shell;
        }

        public async Task LoadAsync()
        {
            Common = null;
            DNA = null;
            UV = null;
            Cuvette = null;
            IsLoading = true;
            var response = await ApiClient.GetCommonParams();
            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();

                return;
            }

            CommonParams.Instance.SetData(response.Data);
            Common = CommonParams.Instance;

            response = await ApiClient.GetDnaParams();
            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                return;
            }

            DnaParams.Instance.SetData(response.Data);
            DNA = DnaParams.Instance;

            response = await ApiClient.GetUvParams();
            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                return;
            }

            UvParams.Instance.SetData(response.Data);
            UV = UvParams.Instance;

            response = await ApiClient.GetCuvetteParams();
            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                return;
            }

            CuvetteParams.Instance.SetData(response.Data);
            Cuvette = CuvetteParams.Instance;

            IsLoading = false;
        }

        private async Task SaveCommonAsync()
        {
            IsLoading = true;
            var response = await ApiClient.SetCommonParams();
            IsLoading = false;
            ErrorHandler(response);
        }

        private async Task SaveDNAAsync()
        {
            IsLoading = true;
            var response = await ApiClient.SetDnaParams();
            IsLoading = false;
            ErrorHandler(response);
        }

        private async Task SaveUVAsync()
        {
            IsLoading = true;
            var response = await ApiClient.SetUvParams();
            IsLoading = false;
            ErrorHandler(response);
        }

        private async Task SaveCuvetteAsync()
        {
            IsLoading = true;
            var response = await ApiClient.SetCuvetteParams();
            IsLoading = false;
            ErrorHandler(response);
        }

        private void ErrorHandler(ApiResponse response)
        {
            if (!response.Result) 
            {
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();
            }
        }

        private void OnBackButton_Click()
        {
            _shell.NavigateToMain();
        }
    }
}
