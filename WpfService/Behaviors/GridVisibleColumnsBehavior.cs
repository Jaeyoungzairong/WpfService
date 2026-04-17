using DevExpress.Xpf.Grid;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace WpfService.Behaviors
{
    public static class GridVisibleColumnsBehavior
    {
        public static readonly DependencyProperty VisibleColumnsProperty =
            DependencyProperty.RegisterAttached(
                "VisibleColumns",
                typeof(IEnumerable),
                typeof(GridVisibleColumnsBehavior),
                new PropertyMetadata(null, OnVisibleColumnsChanged));

        public static void SetVisibleColumns(DependencyObject element, IEnumerable value) 
            => element.SetValue(VisibleColumnsProperty, value);

        public static IEnumerable GetVisibleColumns(DependencyObject element) 
            => (IEnumerable)element.GetValue(VisibleColumnsProperty);

        private static void OnVisibleColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not GridControl grid) return;

            if (e.OldValue is INotifyCollectionChanged oldNotify)
                oldNotify.CollectionChanged -= (_, __) => Apply(grid);

            if (e.NewValue is INotifyCollectionChanged newNotify)
                newNotify.CollectionChanged += (_, __) => Apply(grid);

            Apply(grid);
        }

        private static void Apply(GridControl grid)
        {
            if (grid.View is not TableView) return;

            var list = GetVisibleColumns(grid);
            var names = list?.Cast<object>().Select(x => x?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet()
                        ?? new System.Collections.Generic.HashSet<string>();

            foreach (var col in grid.Columns)
            {
                col.Visible = names.Contains(col.FieldName);
            }
        }
    }
}
