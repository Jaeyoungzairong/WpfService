using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using WpfService.Models;
using WpfService.Services;
using WpfService.ViewModels.Dialogs;

namespace WpfService.ViewModels
{
    public enum HomeTabStatus
    {
        dna,
        proteins,
        cuvette
    }

    public partial class MainViewModel : DialogViewModelBase
    {
        private readonly ShellViewModel _shell;

        public IEnumerable<Assay> SelectedAssays => SelectedTab switch
        {
            HomeTabStatus.dna => Assays.Instance.Dna,
            HomeTabStatus.proteins => Assays.Instance.Proteins,
            HomeTabStatus.cuvette => Assays.Instance.Cuvette,
            _ => Enumerable.Empty<Assay>()
        };

        public HomeTabStatus SelectedTab
        {
            get => GetValue<HomeTabStatus>();
            set
            {
                if (SetValue(value))
                    RaisePropertyChanged(nameof(SelectedAssays));
            }
        }

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public DelegateCommand BackCommand  => new(() => _shell.NavigateToLogin());
        public DelegateCommand ParameterCommand => new(() => _shell.NavigateToParameter());
        public DelegateCommand DeviceCommand => new(() => _shell.NavigateToDevice());
        public DelegateCommand SpectrometerCommand => new(() => _shell.NavigateToSpectrometer());
        public DelegateCommand DataCommand => new(() => _shell.NavigateToData());
        public DelegateCommand LogCommand => new(() => _shell.NavigateToLog());
        public AsyncCommand InformationCommand => new(() => ShowInformationDialog());
        public DelegateCommand<Assay> ChartCommand => new(OnChartButton_Click);

        public MainViewModel(ShellViewModel shell)
        {
            _shell = shell;
            SelectedTab = HomeTabStatus.dna;
        }

        public async Task ShowInformationDialog()
        {
            var viewModel = new InformationDialogViewModel();
            var result = ShowDialog(viewModel, "Information", MessageBoxButton.OK);
            if (result == MessageBoxResult.OK && viewModel.IsSerialNumberChanged)
            {
                IsLoading = true;
                var response = await ApiClient.SetSeirialNumber(viewModel.SerialNumber);
                IsLoading = false;
                if (!response.Result)
                {
                    Dialog.Error(response.ErrMsg);
                    if (response.IsExpired)
                        _shell.NavigateToLogin();
                }
            }
        }

        private void OnChartButton_Click(Assay assay)
        {
            _shell.NavigateToChart(assay.AssayType, assay.AssaySubType);
        }
    }
}
