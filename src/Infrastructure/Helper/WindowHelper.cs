using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AutoGame.Infrastructure.Helper
{
    internal static class WindowHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string sClass, string sWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        // https://weblog.west-wind.com/posts/2020/Oct/12/Window-Activation-Headaches-in-WPF
        public static void ForceForegroundWindow(IntPtr hwnd)
        {
            uint threadId1 = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            uint threadId2 = GetWindowThreadProcessId(hwnd, IntPtr.Zero);

            if (threadId1 != threadId2)
            {
                AttachThreadInput(threadId1, threadId2, true);
                SetForegroundWindow(hwnd);
                AttachThreadInput(threadId1, threadId2, false);
            }
            else
            {
                SetForegroundWindow(hwnd);
            }
        }

        public static bool RepeatTryForceForegroundWindowByTitle(string windowTitle, TimeSpan timeout)
        {
            DateTime start = DateTime.UtcNow;

            while (true)
            {
                IntPtr window = FindWindow(null, windowTitle);

                if (window != IntPtr.Zero)
                {
                    ForceForegroundWindow(window);

                    if (GetForegroundWindow() == window)
                    {
                        return true;
                    }
                }

                if (DateTime.UtcNow - start > timeout)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            return false;
        }
    }
}
