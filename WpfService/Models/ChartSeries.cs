using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace WpfService.Models
{
    public class ChartSeries : BindableBase
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public ObservableCollection<XYPoint> Points { get; set; } = new();

        public Brush Brush { get => GetValue<Brush>(); set => SetValue(value); }
    }

    public class XYPoint
    {
        public double X { get; set; }

        public double Y { get; set; }

        public XYPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
