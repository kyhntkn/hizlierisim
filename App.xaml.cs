using System.Windows;
using System.Windows.Threading;

namespace HizliErisim;

public partial class App : Application
{
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "Hızlı Erişim", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
