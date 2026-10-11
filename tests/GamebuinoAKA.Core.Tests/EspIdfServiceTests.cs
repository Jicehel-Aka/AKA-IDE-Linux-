using System;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class EspIdfServiceTests
    {
        private static (EspIdfService svc, RecordingProcessRunner runner, SettingsService settings) Make()
        {
            var settings = new SettingsService(new TempPlatformPaths());
            settings.Settings.IdfExportScript = OperatingSystem.IsWindows()
                ? @"C:\ESP-IDF\export.bat" : "/home/u/esp/esp-idf/export.sh";
            var runner = new RecordingProcessRunner();
            return (new EspIdfService(settings, runner), runner, settings);
        }
        private static GamebuinoProject P() => new() { FolderPath = "/home/u/ws/jeu", BuildSystem = BuildSystem.EspIdf };

        [Fact]
        public async Task Build_WrapsIdfPyInShell_SourcingExportScript()
        {
            var (svc, runner, _) = Make();
            await svc.BuildAsync(P(), null);
            var req = runner.Last!;
            if (OperatingSystem.IsWindows())
            { Assert.Equal("cmd.exe", req.FileName); Assert.Contains("idf.py -C", req.Arguments[1]); Assert.Contains("build", req.Arguments[1]); }
            else
            { Assert.Equal("bash", req.FileName); Assert.Equal("-lc", req.Arguments[0]); Assert.Contains("source", req.Arguments[1]); Assert.Contains("idf.py -C", req.Arguments[1]); }
        }

        [Fact]
        public async Task Flash_IncludesSerialPort()
        {
            var (svc, runner, settings) = Make();
            settings.Settings.IdfSerialPort = "/dev/ttyUSB0";
            await svc.FlashAsync(P(), null);
            Assert.Contains("-p /dev/ttyUSB0 flash", runner.Last!.Arguments[^1]);
        }

        [Fact] public void DetectSerialPorts_DoesNotThrow() { var (s, _, _) = Make(); Assert.NotNull(s.DetectSerialPorts()); }

        [Fact]
        public void DetectExportScript_ReturnsConfiguredWhenExists()
        {
            var settings = new SettingsService(new TempPlatformPaths());
            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "export-" + Guid.NewGuid().ToString("N") + ".sh");
            System.IO.File.WriteAllText(tmp, "# export");
            settings.Settings.IdfExportScript = tmp;
            try { Assert.Equal(tmp, new EspIdfService(settings, new RecordingProcessRunner()).DetectExportScript()); }
            finally { System.IO.File.Delete(tmp); }
        }

        [Fact]
        public void DetectExportScript_NothingFound_ReturnsEmpty_DoesNotThrow()
        {
            var settings = new SettingsService(new TempPlatformPaths()); // pas de script configuré
            var result = new EspIdfService(settings, new RecordingProcessRunner()).DetectExportScript();
            Assert.NotNull(result); // "" si rien, jamais d'exception
        }
    }
}
