using AutoGame.Infrastructure.Helper;
using AutoGame.Infrastructure.Interfaces;
using System;
using System.Diagnostics;

namespace AutoGame.Infrastructure.Services
{
    public class SteamBigPictureManager : ILauncherManager
    {
        // From: https://www.displayfusion.com/ScriptedFunctions/View/?ID=b21d08ca-438a-41e5-8b9d-0125b07a2abc
        public bool IsRunning => WindowHelper.FindWindow("CUIEngineWin32", "Steam") != IntPtr.Zero;

        public void Start()
        {
            Process.Start(@"C:\Program Files (x86)\Steam\Steam.exe", "-start steam://open/bigpicture");
        }
    }
}
