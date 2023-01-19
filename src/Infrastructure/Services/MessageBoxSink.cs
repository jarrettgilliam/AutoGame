namespace AutoGame.Infrastructure.Services;

using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Serilog.Events;

internal sealed class MessageBoxSink : IMessageBoxSink
{
    public MessageBoxSink(IDialogService dialogService)
    {
        this.DialogService = dialogService;
    }

    private IDialogService DialogService { get; }

    public void Emit(LogEvent logEvent)
    {
        // Only show logs with a level of "Error".
        // "Fatal" errors close the application immediately. No time for a message box.
        // "Warning" and below shouldn't bother the user.
        if (logEvent.Level == LogEventLevel.Error)
        {
            string title = $"{logEvent.Level} {logEvent.RenderMessage()}";
            this.DialogService.ShowMessageBox(new MessageBoxParms
            {
                Message = logEvent.Exception?.ToString() ?? title,
                Title = title,
                Icon = logEvent.Level
            });
        }
    }
}