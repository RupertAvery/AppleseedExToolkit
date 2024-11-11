using System.Windows.Input;

namespace AFSViewer;

public class AsyncCommand<T> : IAsyncCommand<T>
{
    public event EventHandler? CanExecuteChanged;

    private bool _isExecuting;
    private readonly Func<T?, Task> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly IErrorHandler? _errorHandler;

    public AsyncCommand(
        Func<T?, Task> execute,
        Func<bool>? canExecute = null,
        IErrorHandler? errorHandler = null)
    {
        _execute = execute;
        _canExecute = canExecute;
        _errorHandler = errorHandler;
    }

    public bool CanExecute(T? parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async Task ExecuteAsync(T? parameter)
    {
        if (CanExecute(parameter))
        {
            try
            {
                _isExecuting = true;
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
            }
        }

        RaiseCanExecuteChanged();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Explicit implementations
    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute((T?)parameter);
    }

    void ICommand.Execute(object? parameter)
    {
        ExecuteAsync((T?)parameter).FireAndForgetSafeAsync(_errorHandler);
    }
    #endregion
}


public static class TaskUtilities
{
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
    public static async void FireAndForgetSafeAsync(this Task task, IErrorHandler? handler = null)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            handler?.HandleError(ex);
        }
    }
}