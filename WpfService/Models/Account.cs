using DevExpress.Mvvm;

namespace WpfService.Models
{
    public class Account : ViewModelBase
    {
        public static Account Instance { get; } = new Account();
        private Account() { }

        public string User 
        { 
            get => GetValue<string>(); 
            set
            {
                if (SetValue(value))
                {
                    RaisePropertyChanged(nameof(IsMaster));
                    RaisePropertyChanged(nameof(IsManager));
                    RaisePropertyChanged(nameof(IsMasterOrManager));
                    RaisePropertyChanged(nameof(IsUser));
                }
            }
        }
        public bool IsMaster => User == "master";
        public bool IsManager => User == "admin";
        public bool IsMasterOrManager => User == "master" || User == "admin";
        public bool IsUser => User != "master" && User != "admin";
    }
}
