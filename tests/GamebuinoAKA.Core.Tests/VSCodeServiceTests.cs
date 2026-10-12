using System;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class VSCodeServiceTests
    {
        private static (VSCodeService svc, RecordingProcessRunner runner) Make(string? code = "code")
        {
            var runner = new RecordingProcessRunner();
            var tools = new FakeToolLocator();
            if (code != null) tools.Map["code"] = code;
            return (new VSCodeService(new SettingsService(new TempPlatformPaths()), runner, tools), runner);
        }

        [Fact]
        public void OpenProject_LaunchesCode_WithFolderPath()
        { var (s, r) = Make(); s.OpenProject(new GamebuinoProject { FolderPath = "/home/u/ws/mon jeu" }); Assert.Equal("code", r.Last!.FileName); Assert.Equal(new[] { "/home/u/ws/mon jeu" }, r.Last.Arguments); }

        [Fact] public void IsInstalled_TrueWhenLocated() { var (s, _) = Make("code"); Assert.True(s.IsInstalled()); }
        [Fact] public void OpenFolder_NotInstalled_Throws() { var (s, _) = Make(code: null); Assert.Throws<InvalidOperationException>(() => s.OpenFolder("/x")); }
    }
}
