using DevExpress.Mvvm;
using WpfService.Models;
using WpfService.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WpfService.ViewModels.Dialogs
{
    class ParameterDialogViewModel : ViewModelBase
    {
        public string ParamterType { get => GetValue<string>(); set => SetValue(value); }

        public object ParamterData { get => GetValue<object>(); set => SetValue(value); }

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public List<string> ReadOnlyCategories { get => GetValue<List<string>>(); set => SetValue(value); }


        //public List<string> ReadOnlyCategories { get; } = new()
        //{
        //    "1. Common",
        //    "3. Correction(%)"
        //};


        //public AsyncCommand LoadCommand => new(LoadAsync);

        public ParameterDialogViewModel()
        {

        }

        public async Task LoadAsync()
        {
            IsLoading = true;

            if (ParamterType == "DNA")
            {
                var response = await ApiClient.GetDnaParams();
                if (!response.Result)
                {
                    IsLoading = false;
                    Dialog.Error(response.ErrMsg);
                    return;
                }

                DnaParams.Instance.SetData(response.Data);
                ParamterData = DnaParams.Instance;
            }
            else if (ParamterType == "UV")
            {
                var response = await ApiClient.GetUvParams();
                if (!response.Result)
                {
                    IsLoading = false;
                    Dialog.Error(response.ErrMsg);
                    return;
                }

                UvParams.Instance.SetData(response.Data);
                ParamterData = UvParams.Instance;
            }
            else if (ParamterType == "Cuvette")
            {
                var response = await ApiClient.GetCuvetteParams();
                if (!response.Result)
                {
                    IsLoading = false;
                    Dialog.Error(response.ErrMsg);
                    return;
                }

                CuvetteParams.Instance.SetData(response.Data);
                ParamterData = CuvetteParams.Instance;
            }
            else if (ParamterType == "Common")
            {
                var response = await ApiClient.GetCommonParams();
                if (!response.Result)
                {
                    IsLoading = false;
                    Dialog.Error(response.ErrMsg);
                    return;
                }

                CommonParams.Instance.SetData(response.Data);
                ParamterData = CommonParams.Instance;
            }

            IsLoading = false;
        }

        private async Task SaveAsync()
        {
            if (ParamterType == "DNA")
            {
                IsLoading = true;
                var response =  await ApiClient.SetDnaParams();
                IsLoading = false;
                if (!response.Result)
                    Dialog.Error(response.ErrMsg);
            }
            else if (ParamterType == "UV")
            {
                IsLoading = true;
                var response = await ApiClient.SetUvParams();
                IsLoading = false;
                if (!response.Result)
                    Dialog.Error(response.ErrMsg);
            }
            else if (ParamterType == "Cuvette")
            {
                IsLoading = true;
                var response = await ApiClient.SetCuvetteParams();
                IsLoading = false;
                if (!response.Result)
                    Dialog.Error(response.ErrMsg);
            }
            else if (ParamterType == "Common")
            {
                IsLoading = true;
                var response = await ApiClient.SetCommonParams();
                IsLoading = false;
                if (!response.Result)
                    Dialog.Error(response.ErrMsg);
            }
        }
    }
}
