using System.IO;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class SettingsServiceTests
    {
        [Fact]
        public void Load_NoFile_UsesDefaults()
        {
            var svc = new SettingsService(new TempPlatformPaths());
            svc.Load();
            Assert.NotNull(svc.Settings);
            Assert.Equal(10, svc.Settings.MaxRecentProjects);
        }

        [Fact]
        public void Save_Then_Load_RoundTrips()
        {
            var paths = new TempPlatformPaths();
            var a = new SettingsService(paths);
            a.Settings.WorkspaceFolder = "/tmp/ws-xyz";
            a.Settings.EmulatorPath = "/opt/aka/run.sh";
            a.Save();

            var b = new SettingsService(paths);
            b.Load();
            Assert.Equal("/tmp/ws-xyz", b.Settings.WorkspaceFolder);
            Assert.Equal("/opt/aka/run.sh", b.Settings.EmulatorPath);
        }

        [Fact]
        public void Save_CreatesConfigDirectory()
        {
            var paths = new TempPlatformPaths();
            new SettingsService(paths).Save();
            Assert.True(File.Exists(Path.Combine(paths.ConfigDirectory, Path.GetFileName(new SettingsService(paths).SettingsFilePath))));
        }

        [Fact]
        public void Load_InvalidJson_FallsBackToDefaults()
        {
            var paths = new TempPlatformPaths();
            var svc = new SettingsService(paths);
            Directory.CreateDirectory(Path.GetDirectoryName(svc.SettingsFilePath)!);
            File.WriteAllText(svc.SettingsFilePath, "{ ceci n'est pas du json ");
            svc.Load();
            Assert.NotNull(svc.Settings);
        }

        [Fact]
        public void AddRecentProject_DedupsAndCaps()
        {
            var svc = new SettingsService(new TempPlatformPaths());
            svc.Settings.MaxRecentProjects = 3;
            for (int i = 0; i < 5; i++) svc.AddRecentProject("/p" + i);
            svc.AddRecentProject("/p4"); // doublon
            Assert.True(svc.Settings.RecentProjects.Count <= 3);
            Assert.Equal("/p4", svc.Settings.RecentProjects[0]);
        }
    }
}
