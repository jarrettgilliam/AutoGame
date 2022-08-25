namespace AutoGame.Infrastructure.Windows.Services;

using System;
using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Interfaces;

internal sealed class WindowsUser32Service : IUser32Service
{
    public IntPtr FindWindow(string? sClass, string sWindow) =>
        NativeMethods.FindWindow(sClass, sWindow);

    public IntPtr GetForegroundWindow() =>
        NativeMethods.GetForegroundWindow();

    public int SetForegroundWindow(IntPtr hWnd) =>
        NativeMethods.SetForegroundWindow(hWnd);

    public uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId) =>
        NativeMethods.GetWindowThreadProcessId(hWnd, ProcessId);

    public bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach) =>
        NativeMethods.AttachThreadInput(idAttach, idAttachTo, fAttach);

    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string? sClass, string sWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
    }
}