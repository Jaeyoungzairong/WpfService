using DevExpress.Mvvm;
using WpfService.Models;
using WpfService.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfService.ViewModels
{
    public enum MoveMode
    {
        Align,
        Limit,
        Base,
        Move
    }

    public class DeviceViewModel : ViewModelBase
    {
        private readonly ShellViewModel _shell;

        public ICommand LoadCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand MoveCommand { get; }
        public ICommand OpticCommand { get; }
        public ICommand XenonCommand { get; }
        public ICommand TemperatureCommand { get; }

        public MoveMode Mode 
        { 
            get => GetValue<MoveMode>();
            set
            {
                if (SetValue(value))
                    RaisePropertyChanged(nameof(IsMoveValueEnabled));
            }
        }
        public bool IsMoveValueEnabled => Mode == MoveMode.Move;

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public decimal MoveValue { get => GetValue<decimal>(); set => SetValue(value); }

        public string MoveText { get => GetValue<string>(); set => SetValue(value); }

        public string Xenon { get => GetValue<string>(); set => SetValue(value); }

        public decimal Heating { get => GetValue<decimal>(); set => SetValue(value); }

        public string Temperature { get => GetValue<string>(); set => SetValue(value); }

        public DeviceViewModel(ShellViewModel shell)
        {
            _shell = shell;
            Mode = MoveMode.Align;
            Heating = 36.0m;

            //LoadCommand = new AsyncCommand(LoadAsync);
            MoveCommand = new AsyncCommand(MoveAsync);
            OpticCommand = new AsyncCommand<string>(OpticAsync);
            XenonCommand = new AsyncCommand<string>(XenonAsync);
            TemperatureCommand = new AsyncCommand<string>(TemperatureAsync);
            BackCommand = new DelegateCommand(() => { _shell.NavigateToMain(); });
        }

        private async Task MoveAsync()
        {
            IsLoading = true;
            ApiResponse response = null;
            if (Mode == MoveMode.Align)
                response = await ApiClient.MoveAlign();
            else if (Mode == MoveMode.Limit)
                response = await ApiClient.MoveLimit();
            else if (Mode == MoveMode.Base)
                response = await ApiClient.Move(0);
            else if (Mode == MoveMode.Move)
                response = await ApiClient.Move((int)MoveValue);

            IsLoading = false;
            if (response == null)
                return;

            if (!response.Result)
            {
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();
            }
            else
            {
                if (response.Data != null)
                    MoveText = response.Data.ToString();
            }
        }

        private async Task OpticAsync(string type)
        {
            IsLoading = true;
            ApiResponse response = null;
            if (type == "Pedestal")
                response = await ApiClient.MovePedestal();
            else if (type == "Cuvette")
                response = await ApiClient.MoveCuvette();

            IsLoading = false;
            if (response == null)
                return;

            if (!response.Result)
            {
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();
            }
        }

        private async Task XenonAsync(string type)
        {
            IsLoading = true;
            ApiResponse response = null;
            if (type == "Load")
            {
                Xenon = null;
                response = await ApiClient.GetXenon();
            }
            else if (type == "Save")
            {
                if (!int.TryParse(Xenon, out int xenon) || xenon < 10 || xenon > 100)
                {
                    Dialog.Warning("Invalid value.");
                    IsLoading = false;
                    return;
                }
                    
                response = await ApiClient.SetXenon(xenon);
            }
            else if (type == "Test")
            {
                response = await ApiClient.Read(10, 6);
            }

            IsLoading = false;
            if (response == null)
                return;

            if (!response.Result)
            {
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();
            }
            else
            {
                if (type == "Load")
                    Xenon = response.Data.ToString();
            }

            IsLoading = false;
        }

        private async Task TemperatureAsync(string type)
        {
            IsLoading = true;
            ApiResponse response = null;
            if (type == "Set")
                response = await ApiClient.SetTemperature((double)Heating);
            else if (type == "Read")
                response = await ApiClient.GetTemperature();

            IsLoading = false;
            if (response == null)
                return;

            if (!response.Result)
            {
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();
            }
            else
            {
                if (type == "Read")
                    Temperature = response.Data.ToString();
            }
        }

    }
}
