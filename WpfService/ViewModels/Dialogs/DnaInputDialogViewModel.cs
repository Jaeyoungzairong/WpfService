using DevExpress.Mvvm;

namespace WpfService.ViewModels.Dialogs
{
    public class DnaInputDialogViewModel : ViewModelBase
    {
        public decimal CustomFactor { get => GetValue<decimal>(); set => SetValue(value); }

        public bool IsReadOnly { get => GetValue<bool>(); set => SetValue(value); }


        public DnaInputDialogViewModel()
        {
            CustomFactor = 50.00m;
        }
    }
}
