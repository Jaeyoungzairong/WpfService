using DevExpress.Mvvm;

namespace WpfService.ViewModels.Dialogs
{
    public class CuvetteInputDialogViewModel : ViewModelBase
    {
        public decimal RangeFrom { get => GetValue<decimal>(); set => SetValue(value); }

        public decimal RangeTo { get => GetValue<decimal>(); set => SetValue(value); }

        public decimal Wavelength { get => GetValue<decimal>(); set => SetValue(value); }

        public bool IsReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRange { get => GetValue<bool>(); set => SetValue(value); }


        public CuvetteInputDialogViewModel()
        {
            RangeFrom = 200;
            RangeTo = 800;
            Wavelength = 600;
        }
    }
}
