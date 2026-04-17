using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfService.Controls
{
    /// <summary>
    /// BackButton.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BackButton : UserControl
    {
        public BackButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(BackButton));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
    }
}
