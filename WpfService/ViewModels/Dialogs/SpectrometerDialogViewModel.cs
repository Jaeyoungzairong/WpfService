using DevExpress.Mvvm;
using WpfService.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WpfService.ViewModels.Dialogs
{
    public class SpectrometerDialogViewModel : ViewModelBase
    {
        public class StepItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        public List<StepItem> StepOptions { get; }

        public List<string> OpticOptions { get; }

        public decimal ReadCount { get => GetValue<decimal>(); set => SetValue(value); }

        public decimal ReadTime { get => GetValue<decimal>(); set => SetValue(value); }

        public int Step { get => GetValue<int>(); set => SetValue(value); }

        public decimal Xenon { get => GetValue<decimal>(); set => SetValue(value); }

        public string Optic { get => GetValue<string>(); set => SetValue(value); }

        public SpectrometerDialogViewModel()
        {
            OpticOptions = new List<string> { string.Empty, "Pedestal", "Cuvette" };
            StepOptions = new List<StepItem>
            {
                new StepItem { Name = "Standby", Value = 0 },
                new StepItem { Name = "1", Value = 1 },
                new StepItem { Name = "2", Value = 2 },
                new StepItem { Name = "3", Value = 3 },
                new StepItem { Name = "4", Value = 4 },
                new StepItem { Name = "5", Value = 5 }
            };

            ReadCount = 10m;
            ReadTime = 6m;
            Step = 0;
            Xenon = 1.0m;
            Optic = string.Empty;
        } 
    }
}
