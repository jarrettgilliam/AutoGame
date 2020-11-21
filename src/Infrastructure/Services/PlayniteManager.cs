using AutoGame.Infrastructure.Helper;
using AutoGame.Infrastructure.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AutoGame.Infrastructure.Services
{
    public class PlayniteManager : ILauncherManager

    {
        private const string PlayniteFullscreen = "Playnite.FullscreenApp";

        public bool IsRunning => Process.GetProcessesByName(PlayniteFullscreen)?.Any() == true;

        public void Start()
        {
            Process.Start(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Playnite", $"{PlayniteFullscreen}.exe"));
            WindowHelper.RepeatTryForceForegroundWindowByTitle("Playnite", TimeSpan.FromSeconds(5));
        }
    }
}
