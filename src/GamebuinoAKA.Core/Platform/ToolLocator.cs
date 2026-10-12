using System;
using System.Collections.Generic;
using System.IO;

namespace GamebuinoAKA.Core.Platform
{
    public interface IToolLocator
    {
        string? Locate(string toolName, string? configuredPath = null);
    }

    public sealed class ToolLocator : IToolLocator
    {
        public string? Locate(string toolName, string? configuredPath = null)
        {
            if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath)) return configuredPath;
            var onPath = ProbePath(toolName);
            if (onPath != null) return onPath;
            foreach (var candidate in KnownLocations(toolName))
                if (File.Exists(candidate)) return candidate;
            return null;
        }

        private static string? ProbePath(string toolName)
        {
            var pathVar = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVar)) return null;
            var exts = OperatingSystem.IsWindows() ? new[] { "", ".exe", ".cmd", ".bat" } : new[] { "" };
            foreach (var dir in pathVar.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
                foreach (var ext in exts)
                {
                    var candidate = Path.Combine(dir.Trim(), toolName + ext);
                    if (File.Exists(candidate)) return candidate;
                }
            return null;
        }

        private static IEnumerable<string> KnownLocations(string toolName)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (OperatingSystem.IsWindows())
            {
                var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                switch (toolName)
                {
                    case "pio":
                    case "platformio":
                        yield return Path.Combine(home, ".platformio", "penv", "Scripts", "pio.exe");
                        yield return Path.Combine(localApp, "pipx", "venvs", "platformio", "Scripts", "pio.exe");
                        break;
                    case "git":
                        yield return @"C:\Program Files\Git\cmd\git.exe";
                        break;
                    case "code":
                        yield return Path.Combine(localApp, "Programs", "Microsoft VS Code", "bin", "code.cmd");
                        break;
                }
            }
            else
            {
                switch (toolName)
                {
                    case "pio":
                    case "platformio":
                        yield return Path.Combine(home, ".platformio", "penv", "bin", "pio");
                        yield return Path.Combine(home, ".local", "bin", "pio");
                        yield return "/usr/local/bin/pio";
                        break;
                    case "git":
                        yield return "/usr/bin/git";
                        yield return "/usr/local/bin/git";
                        break;
                    case "code":
                        yield return "/usr/bin/code";
                        yield return "/usr/local/bin/code";
                        yield return "/snap/bin/code";
                        break;
                    case "xdg-open":
                        yield return "/usr/bin/xdg-open";
                        break;
                }
            }
        }
    }
}
