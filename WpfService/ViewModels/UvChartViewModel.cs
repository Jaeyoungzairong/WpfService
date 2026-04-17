using DevExpress.Mvvm;
using Newtonsoft.Json.Linq;
using WpfService.Models;
using WpfService.Services;
using WpfService.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfService.ViewModels
{
    public class UvChartViewModel : ChartViewModelBase, IDisposable
    {
        private readonly ShellViewModel _shell;
        private readonly UvInputDialogViewModel _input = new();
        private readonly ParameterDialogViewModel _parameter = new();

        private readonly DataTable _table = new();
        private readonly List<ChartSeries> _allSeries = new();
        private readonly List<int> _selectedOrder = new();

        private readonly DispatcherTimer _blankTimer = new() { Interval = TimeSpan.FromMinutes(30) };

        private ChartMode _mode;
        private int _no = 1;

        public DataView Table { get => GetValue<DataView>(); set => SetValue(value); }

        public ObservableCollection<ChartSeries> VisibleSeries { get; } = new();

        public int TopRowIndex { get => GetValue<int>(); set => SetValue(value); }

        public List<string> VisibleColumns { get => GetValue<List<string>>(); set => SetValue(value); }

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public GridLength ChartColumnWidth { get => GetValue<GridLength>(); set => SetValue(value); }

        public GridLength TableColumnWidth { get => GetValue<GridLength>(); set => SetValue(value); }

        public string ExperimentType { get => GetValue<string>(); set => SetValue(value); }

        public Assay AssayType { get => GetValue<Assay>(); set => SetValue(value); }

        public bool IsDataView
        {
            get => GetValue<bool>();
            set
            {
                if (SetValue(value))
                {
                    RaisePropertyChanged(nameof(IsControlsVisible));
                    RaisePropertyChanged(nameof(IsParameterVisible));
                }
            }
        }
        public bool IsControlsVisible => !IsDataView;
        public bool IsParameterVisible => !IsDataView && Account.Instance.IsMaster;


        public bool IsBlank { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsInput { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsMeasurement { get => GetValue<bool>(); set => SetValue(value); }


        public ICommand LoadCommand { get; }
        public AsyncCommand ParameterCommand => new(ShowParameterDialogAsync);
        public AsyncCommand BlankCommand => new(BlankAsync);
        public DelegateCommand InputCommand => new(ShowInputDialog);
        public AsyncCommand MeasurementCommand => new(MeasurementAsync);
        public AsyncCommand EndExperimentCommand => new(EndExperimentAsync);
        public DelegateCommand BackCommand => new(OnBackButton_Click);
        public DelegateCommand PrevCommand => new(OnPrevButton_Click);
        public DelegateCommand NextCommand => new(OnNextButton_Click);

        public UvChartViewModel(ShellViewModel shell)
        {
            _shell = shell;
            _table.Columns.Add("Selected", typeof(bool));
            _table.Columns.Add("Color", typeof(Brush));
            _table.Columns.Add("Id", typeof(int));
            _table.Columns.Add("No", typeof(int));
            _table.Columns.Add("Sample", typeof(string));
            _table.Columns.Add("Conc", typeof(double));
            _table.Columns.Add("A280", typeof(double));
            _table.Columns.Add("A260", typeof(double));
            _table.Columns.Add("A260/A280", typeof(double));
            NormalChart();

            LoadCommand = new DelegateCommand<(string experimentType, int id)>(p => Load(p.experimentType, p.id));

            _blankTimer.Tick += OnBlank_Expired;
        }

        public async void Load(string experimentType, int id = -1)
        {
            _table.Clear();
            _selectedOrder.Clear();
            _allSeries.Clear();
            VisibleSeries.Clear();
            _no = 1;
            IsBlank = false;
            IsInput = false;
            IsMeasurement = false;
            SampleName = "Sample 1";
            IsDataView = id >= 0;
            ExperimentType = experimentType;
            AssayType = Assays.Instance.GetProteinsAssay(experimentType);
            NormalChart();

            if (!IsDataView)
            {
                IsLoading = true;
                var response = await ApiClient.EndExperiment(ExperimentType);
                IsLoading = false;
                if (!response.Result)
                {
                    Dialog.Error(response.ErrMsg);
                    if (response.IsExpired)
                        _shell.NavigateToLogin();
                    else
                        _shell.NavigateToMain();
                }
                return;
            }

            await LoadDataAsnyc(id);
        }

        public void Dispose()
        {
            _blankTimer.Stop();
            _blankTimer.Tick -= OnBlank_Expired;
        }

        private async Task LoadDataAsnyc(int id)
        {
            IsLoading = true;
            var response = await ApiClient.GetMeasurements(id);
            IsLoading = false;
            if (!response.Result)
            {
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();

                return;
            }

            if (response.Data is JObject obj)
            {
                try
                {
                    var datas = obj.ToObject<Dictionary<string, object>>();
                    var exp = ((JObject)datas["experiment"]).ToObject<Dictionary<string, object>>();
                    var measurements = ((JArray)datas["measurements"]).ToObject<List<Measurement>>();

                    int no = 0;
                    foreach (var measurement in measurements)
                    {
                        var pts = new ObservableCollection<XYPoint>(
                            Enumerable.Range(0, measurement.Data1.Count)
                                      .Select(i => new XYPoint(measurement.Data1[i], measurement.Data2[i]))
                        );


                        _allSeries.Add(new ChartSeries
                        {
                            Id = measurement.Id,
                            Name = measurement.Id.ToString(),
                            Brush = Brushes.Transparent,
                            Points = pts
                        });


                        double conc = (double)measurement.MeasurementData["concentration"];
                        double a280 = (double)measurement.MeasurementData["peak_abs"];
                        double a260 = (double)measurement.MeasurementData["main_abs"];
                        double a260A280 = a260 / a280;

                        _table.Rows.Add(
                            false,
                            Brushes.Transparent,
                            measurement.Id,
                            ++no,
                            measurement.MeasurementName,
                            Math.Round(conc, 3),
                            Math.Round(a280, 3),
                            Math.Round(a260, 3),
                            Math.Round(a260A280, 3));
                    }

                    Table = _table.DefaultView;
                    RefreshVisible();
                    ScrollToLastRow();
                }
                catch (Exception ex)
                {
                    Dialog.Error(ex.ToString());
                }
            }
        }

        private async Task ShowParameterDialogAsync()
        {
            IsLoading = true;
            var response = await ApiClient.GetUvParams();
            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                return;
            }

            UvParams.Instance.SetData(response.Data);
            _parameter.ReadOnlyCategories = new List<string> { "1. Common", "5. Motor" };
            _parameter.ParamterData = UvParams.Instance;

            var result = ShowDialog(_parameter, "Parameter Settings");
            if (result == MessageBoxResult.OK)
            {
                var param = await ApiClient.SetUvParams();
                if (!param.Result)
                {
                    IsLoading = false;
                    Dialog.Error(response.ErrMsg);
                    return;
                }
            }

            IsLoading = false;
        }

        private async Task BlankAsync()
        {
            _blankTimer.Stop();
            IsLoading = true;
            var response = await ApiClient.Blank(ExperimentType);
            IsLoading = false;
            if (!response.Result)
            {
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();

                return;
            }

            IsBlank = true;
            _blankTimer.Start();
        }

        private void ShowInputDialog()
        {
            var factor = _input.SelectedFactor;
            _input.IsReadOnly = IsMeasurement;

            var result = ShowDialog(_input, "Custom Factor");
            if (result == MessageBoxResult.OK)
                IsInput = true;
            else
                _input.SelectedFactor = factor;
        }

        private async Task MeasurementAsync()
        {
            IsLoading = true;
            string factor = _input.Factor.ToString();
            string factorName = _input.SelectedFactor.Key;
            var response = await ApiClient.Sample(ExperimentType, SampleName, factor: factor, factorName: factorName);

            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                return;
            }

            try
            {
                if (response.Data is JObject obj)
                {
                    var datas = obj.ToObject<Dictionary<string, object>>();
                    double conc = Convert.ToDouble(datas["concentration"]);
                    double a280 = Convert.ToDouble(datas["peakAbs"]);
                    double a260 = Convert.ToDouble(datas["mainAbs"]);

                    var waves = ((JArray)datas["waves"]).ToObject<List<double>>();
                    var abs = ((JArray)datas["abs"]).ToObject<List<double>>();
                    var pts = new ObservableCollection<XYPoint>(Enumerable.Range(0, waves.Count).Select(i => new XYPoint(waves[i], abs[i])));
                    _allSeries.Add(new ChartSeries
                    {
                        Id = _no,
                        Name = _no.ToString(),
                        Brush = Brushes.Transparent,
                        Points = pts
                    });


                    _table.Rows.Add(
                        false,
                        Brushes.Transparent,
                        _no,
                        _no,
                        SampleName,
                        Math.Round(conc, 3),
                        Math.Round(a280, 3),
                        Math.Round(a260, 3),
                        Math.Round(a260 / a280, 3));

                    _no++;
                }

                Table = _table.DefaultView;
                RefreshVisible();
                ScrollToLastRow();
                SetNextSampleName();
                IsMeasurement = true;
                IsLoading = false;
            }
            catch (Exception ex)
            {
                IsLoading = false;
                Dialog.Error(ex.ToString());
            }
        }

        private async Task EndExperimentAsync()
        {
            EndExperimentDialogViewModel end = new();
            var result = ShowDialog(end, "End Experiment");
            if (result == MessageBoxResult.OK)
            {
                IsLoading = true;
                var response = await ApiClient.EndExperiment(ExperimentType, end.TitleText);
                IsLoading = false;
                if (!response.Result)
                {
                    Dialog.Error(response.ErrMsg);
                    if (response.IsExpired)
                        _shell.NavigateToLogin();
                }

                _shell.NavigateToMain();
            }
        }

        public void ToggleSelection(int id)
        {
            var row = _table.Rows.Cast<DataRow>().FirstOrDefault(r => Convert.ToInt32(r["Id"].ToString()) == id);
            if (row == null)
                return;

            bool selected = !(bool)row["Selected"];
            if (selected)
            {
                if (_selectedOrder.Count >= 5)
                {
                    Dialog.Warning("You can select up to 5 rows.");
                    return;
                }
                _selectedOrder.Add(id);
            }
            else
            {
                _selectedOrder.Remove(id);
            }

            row["Selected"] = selected;
            ApplySelectionToChart();
        }

        /// <summary>
        /// Selected=true인 것만 차트에 최대 5개 표시(선택 순서 기준)
        /// </summary>
        private void ApplySelectionToChart()
        {
            var active = _selectedOrder.Skip(Math.Max(0, _selectedOrder.Count - 5)).ToList();

            // 1) 테이블 Color 컬럼 갱신
            //    - active에 포함되면 Palette 순서대로 색
            //    - Selected=true지만 active 밖이면 색 없음(Transparent)
            for (int i = 0; i < _table.Rows.Count; i++)
            {
                var row = _table.Rows[i];
                int id = int.Parse(row["Id"].ToString()!);

                bool selected = row.Field<bool>("Selected");
                if (!selected)
                {
                    row["Color"] = Brushes.Transparent;
                    continue;
                }

                int idx = active.IndexOf(id);
                row["Color"] = idx >= 0 ? GetChartBrush(idx, 80) : Brushes.Transparent;
            }

            // 2) 차트 VisibleSeries 갱신 + Brush 동일 적용
            VisibleSeries.Clear();
            if (active.Count == 0)
            {
                var items = _allSeries.Skip(Math.Max(0, _allSeries.Count - 5)).ToList();
                int lastIndex = items.Count - 1;
                for (int i = 0; i < items.Count; i++)
                {
                    var s = items[i];
                    s.Brush = (i == lastIndex) ? GetChartBrush(0) : Brushes.LightGray;
                    VisibleSeries.Add(s);
                }
            }
            else
            {
                for (int i = 0; i < active.Count; i++)
                {
                    int id = active[i];
                    var s = _allSeries.FirstOrDefault(x => x.Id == id);
                    if (s == null) continue;

                    s.Brush = GetChartBrush(i);
                    VisibleSeries.Add(s);
                }
            }

        }

        private void RefreshVisible()
        {
            _selectedOrder.Clear();
            VisibleSeries.Clear();
            var items = _allSeries.Skip(Math.Max(0, _allSeries.Count - 5)).ToList();
            int lastIndex = items.Count - 1;
            for (int i = 0; i < items.Count; i++)
            {
                var s = items[i];
                s.Brush = (i == lastIndex) ? GetChartBrush(0) : Brushes.LightGray;
                VisibleSeries.Add(s);
            }
        }

        private void ScrollToLastRow()
        {
            int lastIndex = Math.Max(0, _table.Rows.Count - 1);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                TopRowIndex = lastIndex;
            }), DispatcherPriority.ContextIdle);
        }

        private void HideChart()
        {
            _mode = ChartMode.Hide;
            VisibleColumns = new List<string> { "No", "Sample", "Conc", "A280", "A260", "A260/A280", };
            ChartColumnWidth = new GridLength(0, GridUnitType.Pixel);
            TableColumnWidth = new GridLength(1, GridUnitType.Star);
        }

        private void NormalChart()
        {
            _mode = ChartMode.Normal;
            VisibleColumns = new List<string> { "No", "Conc", "A280", "A260/A280" };
            ChartColumnWidth = new GridLength(1.5, GridUnitType.Star);
            TableColumnWidth = new GridLength(1, GridUnitType.Star);
        }

        private void FullChart()
        {
            _mode = ChartMode.Full;
            VisibleColumns = new List<string> { "No", "Conc" };
            ChartColumnWidth = new GridLength(3, GridUnitType.Star);
            TableColumnWidth = new GridLength(1, GridUnitType.Star);
        }

        private void OnBlank_Expired(object sender, EventArgs e)
        {
            _blankTimer.Stop();
            IsBlank = false;
            IsInput = false;
            CloseDialogs();
            Dialog.Warning("The measurement time for Blank has exceeded 30 minutes. Please redo the Blank measurement. The input and measurement buttons will be disabled.");
        }

        private void OnBackButton_Click()
        {
            if (IsDataView)
            {
                _shell.OverlayViewModel = null;
                return;
            }
            _shell.NavigateToMain();
        }

        private void OnPrevButton_Click()
        {
            if (_mode == ChartMode.Normal)
                HideChart();
            else if (_mode == ChartMode.Full)
                NormalChart();
        }

        private void OnNextButton_Click()
        {
            if (_mode == ChartMode.Hide)
                NormalChart();
            else if (_mode == ChartMode.Normal)
                FullChart();
        }
    }
}
