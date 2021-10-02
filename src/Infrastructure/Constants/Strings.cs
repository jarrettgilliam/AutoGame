using System;
using System.IO;

namespace AutoGame.Infrastructure.Constants
{
    internal static class Strings
    {
        public static readonly string AppDataFolder =
            Path.Join(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                nameof(AutoGame));
    }
}
