using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

public abstract class ChartViewModelBase : ViewModelBase
{
    private readonly Brush[] _chartPalettes =
    {
        (Brush)new BrushConverter().ConvertFrom("#FF007ACC"),
        Brushes.DarkOrange,
        Brushes.MediumSeaGreen,
        Brushes.MediumPurple,
        Brushes.DeepPink
    };

    protected enum ChartMode
    {
        Full,
        Normal,
        Hide
    }

    public string SampleName { get => GetValue<string>(); set => SetValue(value); }

    protected void SetNextSampleName()
    {
        if (string.IsNullOrWhiteSpace(SampleName))
            SampleName = string.Empty;

        // 숫자 부분만 추출: 문자열 끝에서 숫자 찾기
        int i = SampleName.Length - 1;
        while (i >= 0 && char.IsDigit(SampleName[i]))
        {
            i--;
        }
        // 숫자가 시작하는 위치
        int numberStart = i + 1;
        string prefix = SampleName.Substring(0, numberStart);
        string numberPart = SampleName.Substring(numberStart);

        if (int.TryParse(numberPart, out int number))
            number++; // 숫자 1 증가
        else
            number = 1;

        SampleName = prefix + number.ToString();
    }

    protected Brush GetChartBrush(int index)
    {
        return _chartPalettes[index % _chartPalettes.Length];
    }

    protected Brush GetChartBrush(int index, byte alpha)
    {
        var brush = _chartPalettes[index % _chartPalettes.Length];
        return brush is SolidColorBrush scb
            ? new SolidColorBrush(Color.FromArgb(alpha, scb.Color.R, scb.Color.G, scb.Color.B))
            : brush;
    }

    protected IDialogService DialogService => GetService<IDialogService>("DialogService");

    protected MessageBoxResult ShowDialog(object viewModel, string title = "", MessageBoxButton button = MessageBoxButton.OKCancel)
    {
        if (DialogService == null)
            return MessageBoxResult.None;

        var commands = new List<UICommand> { new UICommand { Id = MessageBoxResult.OK, Caption = "OK", IsDefault = true, IsCancel = false } };

        if (button != MessageBoxButton.OK)
            commands.Add(new UICommand { Id = MessageBoxResult.Cancel, Caption = "Cancel", IsDefault = false, IsCancel = true });

        UICommand result = DialogService.ShowDialog(commands, title, viewModel);
        return result == null ? MessageBoxResult.None : (MessageBoxResult)result.Id;
    }

    protected void CloseDialogs()
    {
        var dialogs = Application.Current.Windows
            .OfType<ThemedWindow>()
            .Where(w => w.IsVisible && w != Application.Current.MainWindow)
            .ToList();

        foreach (var w in dialogs)
            w.Close();
    }
}


