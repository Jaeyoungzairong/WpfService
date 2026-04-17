using DevExpress.Xpf.Grid;
using WpfService.Models;
using WpfService.ViewModels;
using System;
using System.Collections;
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
    public class ExperimentChildNodesSelector : IChildNodesSelector
    {
        public IEnumerable SelectChildren(object item)
        {
            if (item is ExperimentDateGroup g)
                return g.Experiments ?? new List<Experiment>();

            return Array.Empty<object>();
        }
    }

    /// <summary>
    /// DataView.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public partial class DataView : UserControl
    {
        public DataView()
        {
            InitializeComponent();
        }

        private void TreeListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FindParent<Button>(e.OriginalSource as DependencyObject) != null)
                return;

            var view = (TreeListView)sender;

            // 클릭 위치가 어떤 Row인지 얻기
            var hit = view.CalcHitInfo(e.OriginalSource as DependencyObject);
            if (hit == null) return;
            if (hit.RowHandle == GridControl.InvalidRowHandle) return;

            // RowHandle로 Node 가져오기
            var node = view.GetNodeByRowHandle(hit.RowHandle);
            if (node == null) return;

            // ✅ 부모(그룹)만 토글하고 싶으면 여기서 타입 체크
            // node.Content 가 실제 바인딩 데이터(ExperimentDateGroup / Experiment)입니다.
            if (node.Content is ExperimentDateGroup)
            {
                node.IsExpanded = !node.IsExpanded;   // 토글 (또는 view.ExpandNode / view.CollapseNode 사용)
                e.Handled = true;
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T typed) return typed;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}
