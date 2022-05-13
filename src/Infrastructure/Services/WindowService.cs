namespace AutoGame.Infrastructure.Services;

using System;
using AutoGame.Core.Interfaces;

public class WindowService : IWindowService
{
    public WindowService(
        IUser32Service user32Service,
        IDateTimeService dateTimeService,
        ISleepService sleepService)
    {
        this.User32Service = user32Service;
        this.DateTimeService = dateTimeService;
        this.SleepService = sleepService;
    }

    private IUser32Service User32Service { get; }
    private IDateTimeService DateTimeService { get; }
    private ISleepService SleepService { get; }
    
    // https://weblog.west-wind.com/posts/2020/Oct/12/Window-Activation-Headaches-in-WPF
    private void ForceForegroundWindow(IntPtr hwnd)
    {
        uint threadId1 = this.User32Service.GetWindowThreadProcessId(this.User32Service.GetForegroundWindow(), IntPtr.Zero);
        uint threadId2 = this.User32Service.GetWindowThreadProcessId(hwnd, IntPtr.Zero);

        if (threadId1 != threadId2)
        {
            this.User32Service.AttachThreadInput(threadId1, threadId2, true);
            this.User32Service.SetForegroundWindow(hwnd);
            this.User32Service.AttachThreadInput(threadId1, threadId2, false);
        }
        else
        {
            this.User32Service.SetForegroundWindow(hwnd);
        }
    }

    public bool RepeatTryForceForegroundWindowByTitle(string windowTitle, TimeSpan timeout)
    {
        TimeSpan sleepInterval = TimeSpan.FromMilliseconds(100);
        DateTime start = this.DateTimeService.UtcNow;

        while (true)
        {
            IntPtr window = this.User32Service.FindWindow(null, windowTitle);

            if (window != IntPtr.Zero)
            {
                this.ForceForegroundWindow(window);

                if (this.User32Service.GetForegroundWindow() == window)
                {
                    return true;
                }
            }

            if (this.DateTimeService.UtcNow - start > timeout)
            {
                break;
            }

            this.SleepService.Sleep(sleepInterval);
        }

        return false;
    }
}