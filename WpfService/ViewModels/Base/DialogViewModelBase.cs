using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Windows;

public abstract class DialogViewModelBase : ViewModelBase
{
    protected IDialogService DialogService => GetService<IDialogService>("DialogService");

    protected MessageBoxResult ShowDialog(object viewModel, string title = "", MessageBoxButton button = MessageBoxButton.OKCancel)
    {
        if (DialogService == null)
            return MessageBoxResult.None;

        var commands = new List<UICommand> { new UICommand { Id = MessageBoxResult.OK, Caption = "OK", IsDefault = true, IsCancel = false } };

        if (button != MessageBoxButton.OK)
            commands.Add(new UICommand { Id = MessageBoxResult.Cancel, Caption = "Cancel", IsDefault = false, IsCancel = true, AllowCloseWindow = true });

        UICommand result = DialogService.ShowDialog(commands, title, viewModel);
        return result == null ? MessageBoxResult.None : (MessageBoxResult)result.Id;
    }
}
