using System.Collections.Generic;

namespace AutoGame
{
    public static class Constants
    {
        public static string SteamBigPicture = nameof(SteamBigPicture);
        public static string Playnite = nameof(Playnite);

        public static IReadOnlyDictionary<string, string> GameLaunchers { get; } =
            new Dictionary<string, string>()
        {
            { SteamBigPicture, "Steam Big Picture" },
            { Playnite, "Playnite" }
        };
    }
}
