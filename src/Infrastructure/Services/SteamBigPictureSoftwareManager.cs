﻿using Parscript.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Parscript.Infrastructure.Services
{
    public class SteamBigPictureSoftwareManager : ISoftwareManager
    {
        // From: https://www.displayfusion.com/ScriptedFunctions/View/?ID=b21d08ca-438a-41e5-8b9d-0125b07a2abc
        public bool IsRunning => FindWindow("CUIEngineWin32", "Steam") != 0;

        public void Start()
        {
            Process.Start(@"C:\Program Files (x86)\Steam\Steam.exe", "-start steam://open/bigpicture");
        }

        public void Stop()
        {
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string sClass, string sWindow);
    }
}
