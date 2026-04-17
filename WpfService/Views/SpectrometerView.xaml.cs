using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfService.Views
{
    /// <summary>tException: 'DevExpress.Xpf.Core.DXImage'은(는) Setter의 'DevExpress.Xpf.Core.SimpleButton.Glyph' 속성에 대해 올바른 값이 아닙니다.
    /// SpectrometerView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SpectrometerView : UserControl
    {
        public SpectrometerView()
        {
            InitializeComponent();
        }

        private void Chart_CustomDrawCrosshair(object sender, CustomDrawCrosshairEventArgs e)
        {
            const double fontSize = 14;

            foreach (var group in e.CrosshairElementGroups)
            {
                if (group.HeaderElement != null)
                    group.HeaderElement.FontSize = fontSize;

                foreach (var element in group.CrosshairElements)
                {
                    //// 축에 붙는 라벨(X/Y)
                    //if (element.AxisLabelElement != null)
                    //    element.AxisLabelElement.FontSize = fontSize;

                    // 시리즈 값 라벨(박스 내부 한 줄들)
                    if (element.LabelElement != null)
                        element.LabelElement.FontSize = fontSize;
                }
            }
        }

        private void TableView_ShowGridMenu(object sender, DevExpress.Xpf.Grid.GridMenuEventArgs e)
        {
            e.Handled = true;
        }
    }
}
