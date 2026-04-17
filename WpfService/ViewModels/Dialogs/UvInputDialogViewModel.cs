using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace WpfService.ViewModels.Dialogs
{
    public class UvInputDialogViewModel : ViewModelBase
    {
        public Dictionary<string, decimal> Factors { get => GetValue<Dictionary<string, decimal>>(); set => SetValue(value); }

        public KeyValuePair<string, decimal> SelectedFactor
        {
            get => GetValue<KeyValuePair<string, decimal>>();
            //set => RaisePropertiesChanged(nameof(SelectedFactor));
            set
            {
                if (SetValue(value))
                {
                    RaisePropertiesChanged(nameof(IsCustomFactor));
                    Factor = value.Value;
                }
            }
        }
        public bool IsCustomFactor => SelectedFactor.Key == "Custom Factor";

        public decimal Factor { get => GetValue<decimal>(); set => SetValue(value); }

        public bool IsReadOnly { get => GetValue<bool>(); set => SetValue(value); }
        

        public UvInputDialogViewModel()
        {
            Factors = new Dictionary<string, decimal>
            {
                { "1 Abs - 1mg/mL", 1.00m },
                { "BSA", 1.50m },
                { "IgG", 0.72m },
                { "Lysosome", 0.38m },
                { "SA (1.49)", 1.49m },
                { "SA (1.72)", 1.72m },
                { "IgE Human", 0.65m },
                { "Custom Factor", 1.00m },
            };

            SelectedFactor = Factors.First();
        }
    }
}
