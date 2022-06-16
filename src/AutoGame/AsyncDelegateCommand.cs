namespace AutoGame;

using System;
using System.Threading.Tasks;
using System.Windows.Input;

internal sealed class AsyncDelegateCommand : ICommand
{
    public AsyncDelegateCommand(Func<Task> executeMethod)
    {
        this.ExecuteMethod = executeMethod;
    }

    private Func<Task> ExecuteMethod { get; }

    public event EventHandler<Exception>? OnException;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _ = this.ExecuteAsync();

    public async Task ExecuteAsync()
    {
        try
        {
            await this.ExecuteMethod();
        }
        catch (Exception ex)
        {
            this.HandleException(ex);
        }
    }

    private void HandleException(Exception ex)
    {
        EventHandler<Exception>? handler = this.OnException;

        if (handler is null)
        {
            throw new Exception("unhandled exception in async delegate command", ex);
        }

        handler.Invoke(this, ex);
    }
}