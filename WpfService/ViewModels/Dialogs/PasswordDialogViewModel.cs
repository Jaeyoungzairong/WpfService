using DevExpress.Mvvm;

namespace WpfService.ViewModels.Dialogs
{
    public class PasswordDialogViewModel : ViewModelBase
    {
        public string Password { get => GetValue<string>(); set => SetValue(value); }

        public PasswordDialogViewModel()
        {

        }
    }
}
