using DevExpress.Mvvm;
using System;
using System.Windows.Threading;

namespace WpfService.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _clockTimer = new() { Interval = TimeSpan.FromSeconds(1) };

        private object _currentViewModel;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set 
            {
                if (ReferenceEquals(_currentViewModel, value)) 
                    return;

                if (_currentViewModel is IDisposable vm) 
                    vm.Dispose();

                _currentViewModel = value;
                RaisePropertyChanged(nameof(CurrentViewModel));
            }
        }

        private object _overlayViewModel;
        public object OverlayViewModel
        {
            get => _overlayViewModel;
            set 
            {
                if (ReferenceEquals(_overlayViewModel, value)) 
                    return;

                if (_overlayViewModel is IDisposable vm) 
                    vm.Dispose();

                if (SetProperty(ref _overlayViewModel, value, nameof(OverlayViewModel)))
                    RaisePropertyChanged(nameof(IsOverlayVisible));
            }
        }
        public bool IsOverlayVisible => OverlayViewModel != null;

        public string NowText { get => GetValue<string>(); set => SetValue(value); }

        public LoginViewModel LoginVM { get; }
        public MainViewModel MainVM { get; }
        public RecoveryViewModel RecoveryVM { get; }
        public ParameterViewModel ParameterVM { get; }
        public SpectrometerViewModel SpectrometerVM { get; }
        public DeviceViewModel DeviceVM { get; }
        public LogViewModel LogVM { get; }
        public DnaChartViewModel DnaChartVM { get; }
        public UvChartViewModel UvChartVM { get; }
        public CuvetteChartViewModel CuvetteChartVM { get; }
        public DataViewModel DataVM { get; }

        //public DelegateCommand CloseOverlayCommand => new(() => OverlayViewModel = null);

        public ShellViewModel()
        {
            LoginVM = new LoginViewModel(this);
            MainVM = new MainViewModel(this);
            RecoveryVM = new RecoveryViewModel(this);
            ParameterVM = new ParameterViewModel(this);
            DeviceVM = new DeviceViewModel(this);
            SpectrometerVM = new SpectrometerViewModel(this);
            LogVM = new LogViewModel(this);
            DnaChartVM = new DnaChartViewModel(this);
            UvChartVM = new UvChartViewModel(this);
            CuvetteChartVM = new CuvetteChartViewModel(this);
            DataVM = new DataViewModel(this);

            ((ISupportParentViewModel)LoginVM).ParentViewModel = this;
            ((ISupportParentViewModel)MainVM).ParentViewModel = this;
            ((ISupportParentViewModel)SpectrometerVM).ParentViewModel = this;
            ((ISupportParentViewModel)DnaChartVM).ParentViewModel = this;
            ((ISupportParentViewModel)UvChartVM).ParentViewModel = this;
            ((ISupportParentViewModel)CuvetteChartVM).ParentViewModel = this;

            CurrentViewModel = LoginVM;

            _clockTimer.Tick += (_, __) => NowText = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            _clockTimer.Start();
        }

        public void NavigateToLogin() => CurrentViewModel = LoginVM;
        public void NavigateToMain() => CurrentViewModel = MainVM;
        public void NavigateToRecovery() => CurrentViewModel = RecoveryVM;
        public void NavigateToParameter() => CurrentViewModel = ParameterVM;
        public void NavigateToDevice() => CurrentViewModel = DeviceVM;
        public void NavigateToSpectrometer() => CurrentViewModel = SpectrometerVM;
        public void NavigateToLog() => CurrentViewModel = LogVM;
        public void NavigateToData() => CurrentViewModel = DataVM;
        public void NavigateToChart(string assayType, string experimentType, int experimentId = -1)
        {
            string type = assayType.ToLower();
            bool isDataViewer = experimentId >= 0;
            if (type == "dna")
            {
                DnaChartVM.Load(experimentType, experimentId);
                if (isDataViewer)
                    OverlayViewModel = DnaChartVM;
                else
                    CurrentViewModel = DnaChartVM;
            }
            else if (type == "proteins")
            {
                if (experimentType == "DirectUV")
                {
                    UvChartVM.Load(experimentType, experimentId);
                    if (isDataViewer)
                        OverlayViewModel = UvChartVM;
                    else
                        CurrentViewModel = UvChartVM;
                }
            }
            else if (type == "cuvette")
            {
                CuvetteChartVM.Load(experimentType, experimentId);
                if (isDataViewer)
                    OverlayViewModel = CuvetteChartVM;
                else
                    CurrentViewModel = CuvetteChartVM;
            }
            //else
            //{
            //    ChartVM.Load(assayType, experimentType, experimentId);
            //    CurrentViewModel = ChartVM;
            //}

        }
    }
}
