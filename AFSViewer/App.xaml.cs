using System.Configuration;
using System.Data;
using System.Windows;

namespace AFSViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
    }

    void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "Unhandled Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        e.Handled = true;
    }
}

