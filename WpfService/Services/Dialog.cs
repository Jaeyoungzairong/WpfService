using DevExpress.Xpf.Core;
using System.Windows;
using System.Windows.Controls;

namespace WpfService.Services
{
    public class Dialog
    {
        public static MessageBoxResult Error(string message, string title = "Error", MessageBoxButton button = MessageBoxButton.OK) 
            => Show(message, title, MessageBoxImage.Error, button);

        public static MessageBoxResult Warning(string message, string title = "Warning", MessageBoxButton button = MessageBoxButton.OK) 
            => Show(message, title, MessageBoxImage.Warning, button);

        public static MessageBoxResult Info(string message, string title = "Info", MessageBoxButton button = MessageBoxButton.OK) 
            => Show(message, title, MessageBoxImage.Information, button);

        private static MessageBoxResult Show(string message, string title, MessageBoxImage icon, MessageBoxButton button)
        {
            var owner = Application.Current?.MainWindow;

            if (owner == null)
                return MessageBoxResult.None;

            var content = new TextBlock
            {
                Text = message,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(8)
            };

            return ThemedMessageBox.Show(
                owner: owner,
                title: title,
                messageContent: content,
                messageBoxButtons: button,
                icon: icon
            );
        }
    }
}
