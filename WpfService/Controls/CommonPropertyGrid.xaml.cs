using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.PropertyGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace WpfService.Controls
{
    /// <summary>
    /// CommonPropertyGrid.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommonPropertyGrid : UserControl
    {
        public CommonPropertyGrid()
        {
            InitializeComponent();
        }

        public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        //public static readonly DependencyProperty SelectedObjectProperty =
        //    DependencyProperty.Register(
        //        nameof(SelectedObject),
        //        typeof(object),
        //        typeof(CommonPropertyGrid),
        //        new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedObjectProperty =
    DependencyProperty.Register(
        nameof(SelectedObject),
        typeof(object),
        typeof(CommonPropertyGrid),
        new PropertyMetadata(null, OnSelectedObjectChanged));

        public IEnumerable<string> ReadOnlyCategories
        {
            get => (IEnumerable<string>)GetValue(ReadOnlyCategoriesProperty);
            set => SetValue(ReadOnlyCategoriesProperty, value);
        }

        public static readonly DependencyProperty ReadOnlyCategoriesProperty =
            DependencyProperty.Register(
                nameof(ReadOnlyCategories),
                typeof(IEnumerable<string>),
                typeof(CommonPropertyGrid),
                new PropertyMetadata(null, OnReadOnlyCategoriesChanged));

        private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CommonPropertyGrid control)
                control.BuildPropertyDefinitions();
        }

        private static void OnReadOnlyCategoriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CommonPropertyGrid control)
                control.BuildPropertyDefinitions();
        }

        private void BuildPropertyDefinitions()
        {
            if (propertyGrid == null)
                return;

            propertyGrid.PropertyDefinitions.Clear();

            if (SelectedObject == null)
                return;

            var readOnlySet = new HashSet<string>(
                ReadOnlyCategories ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            var props = SelectedObject.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var prop in props)
            {
                var category = prop.GetCustomAttribute<CategoryAttribute>()?.Category;
                bool isReadOnly = !string.IsNullOrWhiteSpace(category) && readOnlySet.Contains(category);

                var def = new PropertyDefinition
                {
                    Path = prop.Name,
                    IsReadOnly = isReadOnly
                };

                if (prop.PropertyType == typeof(bool))
                {
                    def.CellTemplate = (DataTemplate)FindResource("BoolCellTemplate");
                }
                else
                {
                    def.EditSettings = new TextEditSettings
                    {
                        HorizontalContentAlignment = (EditSettingsHorizontalAlignment)HorizontalAlignment.Center
                    };
                }

                propertyGrid.PropertyDefinitions.Add(def);
            }
        }
    }
}

