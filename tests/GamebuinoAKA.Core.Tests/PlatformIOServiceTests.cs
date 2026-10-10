using System;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class PlatformIOServiceTests
    {
        private static (PlatformIOService svc, RecordingProcessRunner runner) Make(string? pio = "pio")
        {
            var settings = new SettingsService(new TempPlatformPaths());
            var runner = new RecordingProcessRunner();
            var tools = new FakeToolLocator();
            if (pio != null) tools.Map["pio"] = pio;
            return (new PlatformIOService(settings, runner, tools), runner);
        }
        private static GamebuinoProject P() => new() { FolderPath = "/home/u/ws/game", BuildSystem = BuildSystem.PlatformIO };

        [Fact] public async Task Build_RunsPioRun_InProjectDir()
        { var (s, r) = Make(); await s.BuildAsync(P(), null); Assert.Equal("pio", r.Last!.FileName); Assert.Equal(new[] { "run" }, r.Last.Arguments); Assert.Equal("/home/u/ws/game", r.Last.WorkingDirectory); }
        [Fact] public async Task Flash_RunsUpload()
        { var (s, r) = Make(); await s.FlashAsync(P(), null); Assert.Equal(new[] { "run", "-t", "upload" }, r.Last!.Arguments); }
        [Fact] public async Task Monitor_RunsDeviceMonitor()
        { var (s, r) = Make(); await s.MonitorAsync(P(), null); Assert.Equal(new[] { "device", "monitor" }, r.Last!.Arguments); }
        [Fact] public async Task Clean_RunsRunTClean()
        { var (s, r) = Make(); await s.CleanAsync(P(), null); Assert.Equal(new[] { "run", "-t", "clean" }, r.Last!.Arguments); }
        [Fact] public async Task NoPio_Throws()
        { var (s, _) = Make(pio: null); await Assert.ThrowsAsync<InvalidOperationException>(() => s.BuildAsync(P(), null)); }
        [Fact] public void DetectPioPath_ReturnsLocated()
        { var (s, _) = Make("pio"); Assert.Equal("pio", s.DetectPioPath()); }
    }
}
