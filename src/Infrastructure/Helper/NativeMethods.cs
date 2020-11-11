using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AutoGame.Infrastructure.Helper
{
    static class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string sClass, string sWindow);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);
    }
}
