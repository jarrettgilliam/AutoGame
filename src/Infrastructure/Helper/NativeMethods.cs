using System;
using System.Runtime.InteropServices;

namespace AutoGame.Infrastructure.Helper
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string sClass, string sWindow);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);
    }
}
