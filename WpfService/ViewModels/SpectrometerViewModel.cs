using DevExpress.Mvvm;
using Newtonsoft.Json.Linq;
using WpfService.Models;
using WpfService.Services;
using WpfService.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace WpfService.ViewModels
{
    public class SpectrometerViewModel : DialogViewModelBase
    {
        private readonly ShellViewModel _shell;
        private List<double> _waves;
        private readonly SpectrometerDialogViewModel _settings = new SpectrometerDialogViewModel();

        private readonly DataTable _table = new();

        private bool _isCancellationRequested = false;
        private bool _isCapture = false;

        public bool IsRunning { get => GetValue<bool>(); set => SetValue(value); }

        public ObservableCollection<XYPoint> Data 
        { 
            get => GetValue<ObservableCollection<XYPoint>>(); 
            set => SetValue(value); 
        }

        public ObservableCollection<XYPoint> Capture 
        { 
            get => GetValue<ObservableCollection<XYPoint>>();
            set 
            {
                if (!SetValue(value)) return;

                _table.Clear();
                if (value != null)
                {
                    for (int i = 0; i < value.Count; i++)
                        _table.Rows.Add(i + 1, value[i].X, value[i].Y);
                }
                RaisePropertyChanged(nameof(CanClear));
                RaisePropertyChanged(nameof(Table));
            } 
        }
        public bool CanClear => Capture != null;
        public DataView Table => _table.DefaultView;

        public bool IsGridMode { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public AsyncCommand LoadCommand => new(LoadAsync);
        public AsyncCommand ReadCommand => new(ReadAsync, () => true, true);
        public AsyncCommand CaptureCommand => new(CaptureAsync);
        public AsyncCommand SettingCommand => new(OpenSettingAsync);
        public DelegateCommand ViewCommand => new(OnViewButton_Click);
        public DelegateCommand ClearCommand => new(OnClearButton_Click);
        public DelegateCommand BackCommand => new(OnBackButton_Click);

        public SpectrometerViewModel(ShellViewModel shell)
        {
            _shell = shell;
            _table.Columns.Add("Index", typeof(int));
            _table.Columns.Add("Wave", typeof(double));
            _table.Columns.Add("Data", typeof(double));
        }

        private async Task LoadAsync()
        {
            IsLoading = true;
            Data = null;
            Capture = null;
            _settings.Step = 0;

            var response = await ApiClient.GetWave();
            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();

                return;
            }

            if (response.Data is JArray jArray)
                _waves = jArray.ToObject<List<double>>();

            response = await ApiClient.GetXenon();
            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                return;
            }

            try { 
                _settings.Xenon = Convert.ToInt32(response.Data) / 100.0m; 
            }
            catch { }
            
            response = await ApiClient.MoveStep(_settings.Step);
            IsLoading = false;
            if (!response.Result)
                Dialog.Error(response.ErrMsg);
        }

        private async Task ReadAsync()
        {
            if (_waves == null)
            {
                Dialog.Error("No wavelength data");
                return;
            }
            else if (IsRunning)
            {
                _isCancellationRequested = true;
                return;
            }

            _isCancellationRequested = false;
            _isCapture = false;
            int readCount = (int)_settings.ReadCount;
            int readTime = (int)_settings.ReadTime;
            IsRunning = true;
            List<double> data = null;
            ApiResponse response = null;
            
            while (!_isCancellationRequested)
            {
                response = await ApiClient.Read(readCount, readTime);
                if (response.Result)
                {
                    if (response.Data is JArray jArray)
                        data = jArray.ToObject<List<double>>();

                    var newPoints = new List<XYPoint>();
                    for (int i = 0; i < _waves.Count; i++)
                        newPoints.Add(new XYPoint(_waves[i], data[i]));

                    Data = new ObservableCollection<XYPoint>(newPoints);

                    //for (int i = 0; i < _waves.Count; i++)
                    //    Data.Add(new XYPoint(_waves[i], data[i]));

                    if (_isCapture)
                    {
                        Capture = new ObservableCollection<XYPoint>(newPoints);
                        _isCapture = false;
                    }

                    await Task.Delay(200);
                }
                else
                {
                    _isCancellationRequested = true;
                    Dialog.Error(response.ErrMsg);
                }
            }
            

            IsRunning = false;
            if (response?.IsExpired ?? false)
                _shell.NavigateToLogin();
        }

        private async Task CaptureAsync()
        {
            if (_waves == null)
            {
                Capture = null;
                Dialog.Error("No wavelength data");
                return;
            }

            if (IsRunning)
            {
                _isCapture = true;
                return;
            }

            int readCount = (int)_settings.ReadCount;
            int readTime = (int)_settings.ReadTime;
            var response = await ApiClient.Read(readCount, readTime);
            if (!response.Result)
            {
                Capture = null;
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();

                return;
            }

            if (response.Data is JArray jArray)
            {
                List<double> data = jArray.ToObject<List<double>>();
                var newPoints = new List<XYPoint>();
                for (int i = 0; i < _waves.Count; i++)
                    newPoints.Add(new XYPoint(_waves[i], data[i]));

                Capture = new ObservableCollection<XYPoint>(newPoints);
            }
        }

        private async Task OpenSettingAsync()
        {
            _settings.Optic = string.Empty;
            var result = ShowDialog(_settings, "Settings");
            if (result == MessageBoxResult.OK)
            {
                IsLoading = true;
                int xenon = (int)(_settings.Xenon * 100);
                var response = await ApiClient.SetXenon(xenon);
                if (!response.Result)
                {
                    IsLoading = false;
                    Dialog.Error(response.ErrMsg);
                    if (response.IsExpired)
                        _shell.NavigateToLogin();

                    return;
                }

                if (_settings.Optic == "Pedestal")
                    await ApiClient.MovePedestal();
                else if (_settings.Optic == "Cuvette")
                    await ApiClient.MoveCuvette();

                response = await ApiClient.MoveStep(_settings.Step);
                IsLoading = false;
                if (!response.Result)
                    Dialog.Error(response.ErrMsg);
            }
        }

        private void OnViewButton_Click()
        {
            if (IsRunning)
                _isCancellationRequested = true;

            IsGridMode = !IsGridMode;
        }

        private void OnClearButton_Click()
        {
            Capture = null;
            if (!IsRunning)
                Data = null;
        }
        private void OnBackButton_Click()
        {
            _isCancellationRequested = true;
            _shell.NavigateToMain();
        }
    }
}
