using System.Windows.Input;

namespace AFSViewer;

public interface IAsyncCommand<in T> : ICommand
{
    Task ExecuteAsync(T? parameter);
    bool CanExecute(T? parameter);
}