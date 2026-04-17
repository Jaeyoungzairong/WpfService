using DevExpress.Mvvm;

namespace WpfService.ViewModels.Dialogs
{
    public class EndExperimentDialogViewModel : ViewModelBase
    {
        public string TitleText { get => GetValue<string>(); set => SetValue(value); }

        public EndExperimentDialogViewModel()
        {
            TitleText = string.Empty;
        }
    }
}
