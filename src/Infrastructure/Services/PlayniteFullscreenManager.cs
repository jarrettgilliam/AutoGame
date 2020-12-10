using AutoGame.Infrastructure.Helper;
using AutoGame.Infrastructure.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AutoGame.Infrastructure.Services
{
    public class PlayniteFullscreenManager : ISoftwareManager
    {
        private const string PLAYNITE_FULLSCREEN_APP = "Playnite.FullscreenApp";

        public string Key { get; } = "PlayniteFullscreen";

        public string Description { get; } = "Playnite Fullscreen";

        public bool IsRunning => Process.GetProcessesByName(PLAYNITE_FULLSCREEN_APP)?.Any() == true;

        public void Start(string softwarePath)
        {
            Process.Start(softwarePath);
            WindowHelper.RepeatTryForceForegroundWindowByTitle("Playnite", TimeSpan.FromSeconds(5));
        }

        public string FindSoftwarePathOrDefault()
        {
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Playnite", $"{PLAYNITE_FULLSCREEN_APP}.exe");
        }
    }
}
