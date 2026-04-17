using DevExpress.Mvvm;
using DevExpress.Xpf.Dialogs;
using Newtonsoft.Json.Linq;
using WpfService.Models;
using WpfService.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WpfService.ViewModels
{
    public class DataViewModel : ViewModelBase
    {
        private readonly ShellViewModel _shell;
        private List<ExperimentDateGroup> _allDateGroups = new();

        public List<string> AssayOptions { get; } = new() { "", "DNA", "Proteins", "Cuvette" };

        public ObservableCollection<ExperimentDateGroup> DateGroups
        {
            get => GetValue<ObservableCollection<ExperimentDateGroup>>();
            set => SetValue(value);
        }
        
        public string SelectedAssay
        {
            get => GetValue<string>();
            set 
            {
                if (SetValue(value))
                    ApplyAssayTypeFilter(value);
            }
        }

        public DateTime FromDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime ToDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public AsyncCommand LoadCommand => new(LoadAsync);
        public AsyncCommand SearchCommand => new(LoadDataAsync);
        public AsyncCommand DownloadCommand => new(DownloadAsync);
        public DelegateCommand BackCommand => new(OnBackButton_Click);
        public DelegateCommand<Experiment> DetailCommand => new(OnDetailButton_Click);

        public DataViewModel(ShellViewModel shell)
        {
            _shell = shell;
        }

        public async Task LoadAsync()
        {
            FromDate = DateTime.Now.Date.AddYears(-1);
            ToDate = DateTime.Now.Date;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            DateTimeRange timeRange = new(FromDate, ToDate.AddDays(1));
            if (!timeRange.IsValid)
            {
                Dialog.Warning("Invalid date range.");
                return;
            }

            IsLoading = true;
            var response = await ApiClient.GetExperiments(timeRange.Start, timeRange.End);
            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();

                return;
            }

            try
            {
                _allDateGroups.Clear();
                var groups = ((JArray)response.Data).ToObject<List<ExperimentDateGroup>>();
                foreach (var grp in groups)
                    _allDateGroups.Add(grp);

                ApplyAssayTypeFilter(SelectedAssay);

                IsLoading = false;
            }
            catch (Exception ex)
            {
                IsLoading = false;
                Dialog.Error(ex.ToString());
            }
        }

        private async Task DownloadAsync()
        {
            var dlg = new DXSaveFileDialog
            {
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                Filter = "Database (*.db)|*.db",
                DefaultExt = ".db",
                FileName = "nabi_pro_scipher.db",
                AddExtension = true
            };

            if (dlg.ShowDialog() != true)
                return;

            IsLoading = true;
            try
            {
                var response = await ApiClient.GetDB();
                if (!response.Result)
                {
                    Dialog.Error(response.ErrMsg);
                    if (response.IsExpired)
                        _shell.NavigateToLogin();
                }
                else if (response.Data == null)
                {
                    Dialog.Error("No data downloaded");
                }
                else
                {
                    string json = response.Data.ToString();
                    byte[] bytes = Convert.FromBase64String(json);
                    File.WriteAllBytes(dlg.FileName, bytes);
                }
            }
            catch (Exception ex)
            {
                Dialog.Error(ex.ToString());
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyAssayTypeFilter(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                DateGroups = new ObservableCollection<ExperimentDateGroup>(_allDateGroups);
                return;
            }

            List<ExperimentDateGroup> groups = new List<ExperimentDateGroup>();
            foreach (var g in _allDateGroups)
            {
                var experiments = g.Experiments.Where(x => x.AssayType == type).ToList();
                if (experiments.Count > 0)
                {
                    var data = new ExperimentDateGroup();
                    data.Date = g.Date;
                    data.Experiments = experiments;
                    groups.Add(data);
                }
            }

            DateGroups = new ObservableCollection<ExperimentDateGroup>(groups);
        }

        private void OnBackButton_Click()
        {
            _shell.NavigateToMain();
        }

        private void OnDetailButton_Click(Experiment exp)
        {
            _shell.NavigateToChart(exp.AssayType, exp.ExperimentType, exp.Id);
        }
    }
}
