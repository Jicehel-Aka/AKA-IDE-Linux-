using System;
using GamebuinoAKA.Core.Platform;

namespace GamebuinoAKA.App.Platform
{
    public static class PlatformPathsFactory
    {
        public static IPlatformPaths Create() =>
            OperatingSystem.IsWindows()
                ? new WindowsPlatformPaths()
                : new LinuxPlatformPaths();
    }
}
