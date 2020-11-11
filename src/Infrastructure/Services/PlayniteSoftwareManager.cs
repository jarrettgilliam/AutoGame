using AutoGame.Infrastructure.Helper;
using AutoGame.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoGame.Infrastructure.Services
{
    public class PlayniteSoftwareManager : ISoftwareManager
    {
        private const string PlayniteFullscreen = "Playnite.FullscreenApp";

        public bool IsRunning => Process.GetProcessesByName(PlayniteFullscreen)?.Any() == true;

        public void Start()
        {
            Process p = Process.Start(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Playnite", $"{PlayniteFullscreen}.exe"));
            ////NativeMethods.SetForegroundWindow(p.MainWindowHandle);
        }
    }
}
