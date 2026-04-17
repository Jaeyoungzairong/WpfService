using DevExpress.Mvvm;
using DevExpress.Xpf.Dialogs;
using Newtonsoft.Json.Linq;
using WpfService.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfService.ViewModels
{
    public class LogViewModel : ViewModelBase
    {

        private readonly ShellViewModel _shell;

        public List<string> DateList { get => GetValue<List<string>>(); set => SetValue(value); }

        public bool IsLoading { get => GetValue<bool>(); set => SetValue(value); }

        public string SelectedDate 
        { 
            get => GetValue<string>();
            set 
            { 
                if (SetValue(value)) 
                    _ = LoadTextAsync();
            }
        }

        public string LogText { get => GetValue<string>(); set => SetValue(value); }

        public AsyncCommand LoadCommand => new(LoadAsync);
        public AsyncCommand DownloadCommand => new(DownloadAsncy);
        public DelegateCommand BackCommand => new(OnBackButton_Click);

        public LogViewModel(ShellViewModel shell)
        {
            _shell = shell;
        }

        private async Task LoadAsync()
        {
            SelectedDate = null;
            IsLoading = true;
            var response = await ApiClient.GetLogDates();
            if (!response.Result)
            {
                IsLoading = false;
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();

                return;
            }

            if (response.Data is JArray arr)
            {
                DateList = arr.ToObject<List<string>>();
                if (DateList.Count > 0)
                    SelectedDate = DateList.First();
            }

            IsLoading = false;
        }

        private async Task LoadTextAsync()
        {
            if (string.IsNullOrEmpty(SelectedDate))
            {
                LogText = null;
                return;
            }

            IsLoading = true;
            var response = await ApiClient.GetLogText(SelectedDate);
            IsLoading = false;
            if (!response.Result)
            {
                Dialog.Error(response.ErrMsg);
                if (response.IsExpired)
                    _shell.NavigateToLogin();
            }
            else
            {
                LogText = response.Data.ToString();
            }
        }

        private async Task DownloadAsncy()
        {
            string fileName = $"Log_{DateTime.Now:yyMMddHHmmss}.zip"; 
            var dlg = new DXSaveFileDialog
            {
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                Filter = "Zip (*.zip)|*.zip",
                DefaultExt = ".zip",
                FileName = fileName,
                AddExtension = true
            };

            if (dlg.ShowDialog() != true)
                return;

            IsLoading = true;
            try
            {
                if (File.Exists(dlg.FileName))
                    File.Delete(dlg.FileName);

                // DateList가 1개여도 zip 생성되도록(기존 코드는 files.Count > 1일때만 zip 생성)
                using (ZipArchive zip = ZipFile.Open(dlg.FileName, ZipArchiveMode.Create))
                {
                    foreach (var date in DateList)
                    {
                        var response = await ApiClient.GetLogText(date);
                        if (!response.Result)
                        {
                            Dialog.Error(response.ErrMsg);
                            return;
                        }
                        // 엔트리 생성 후, 디스크 파일 없이 바로 쓰기
                        var entry = zip.CreateEntry($"{date}.txt", CompressionLevel.Optimal);
                        using (var entryStream = entry.Open())
                        using (var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
                        {
                            // response.Data가 string이 아닐 수도 있으니 안전하게 문자열화
                            writer.Write(response.Data?.ToString() ?? string.Empty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dialog.Error(ex.ToString());
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnBackButton_Click()
        {
            _shell.NavigateToMain();
        }
    }
}
