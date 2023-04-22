namespace AutoGame.Infrastructure.Windows.Interfaces;

using System;
using AutoGame.Infrastructure.Windows.Enums;

public interface IUser32Service
{
    IntPtr FindWindow(string? sClass, string sWindow);
    IntPtr GetForegroundWindow();
    int SetForegroundWindow(IntPtr hwnd);
    uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
    bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
    bool ShowWindowAsync(IntPtr hWnd, ShowWindowCommands nCmdShow);
}