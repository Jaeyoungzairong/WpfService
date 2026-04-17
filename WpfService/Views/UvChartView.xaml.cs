using DevExpress.Xpf.Grid;
using WpfService.ViewModels;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfService.Views
{
    /// <summary>
    /// UvChartView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UvChartView : UserControl
    {
        public UvChartView()
        {
            InitializeComponent();
        }

        private void gridControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (gridControl.View is not TableView view)
                return;

            var hit = view.CalcHitInfo(e.OriginalSource as DependencyObject);
            if (!hit.InRow || hit.RowHandle < 0)
                return;

            if (gridControl.GetRow(hit.RowHandle) is not DataRowView row)
                return;


            if (!int.TryParse(row["Id"]?.ToString(), out int id))
                return;

            if (DataContext is UvChartViewModel vm)
                vm.ToggleSelection(id); // 네가 만든 함수로

            e.Handled = true;
        }
    }
}
