using System;
using System.IO;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class EmulatorServiceTests
    {
        private static (EmulatorService svc, RecordingProcessRunner runner, string root) Make(string? emuPath)
        {
            var paths = new TempPlatformPaths();
            var settings = new SettingsService(paths);
            var runner = new RecordingProcessRunner();
            if (emuPath != null)
            {
                settings.Settings.EmulatorPath = emuPath;
                Directory.CreateDirectory(Path.GetDirectoryName(emuPath)!);
                if (!File.Exists(emuPath)) File.WriteAllText(emuPath, "#!/bin/sh\n");
            }
            return (new EmulatorService(settings, runner, new FakeToolLocator()), runner, paths.Root);
        }

        private static string Proj(string root, string name, Action<string> populate)
        { var d = Path.Combine(root, name); Directory.CreateDirectory(d); populate(d); return d; }

        [Fact]
        public void ResolveFirmware_EspIdf_PrefersProjectNamedBin()
        {
            var (svc, _, root) = Make(null);
            var dir = Proj(root, "MonJeu", p => {
                var b = Path.Combine(p, "build"); Directory.CreateDirectory(b);
                File.WriteAllBytes(Path.Combine(b, "bootloader.bin"), new byte[100]);
                File.WriteAllBytes(Path.Combine(b, "MonJeu.bin"), new byte[500]);
            });
            Assert.Equal(Path.Combine(dir, "build", "MonJeu.bin"),
                svc.ResolveFirmware(new GamebuinoProject { Name = "MonJeu", FolderPath = dir }));
        }

        [Fact]
        public void ResolveFirmware_EspIdf_FallsBackToLargestNonSystemBin()
        {
            var (svc, _, root) = Make(null);
            var dir = Proj(root, "Sans", p => {
                var b = Path.Combine(p, "build"); Directory.CreateDirectory(b);
                File.WriteAllBytes(Path.Combine(b, "bootloader.bin"), new byte[9000]);
                File.WriteAllBytes(Path.Combine(b, "app.bin"), new byte[500]);
            });
            Assert.Equal(Path.Combine(dir, "build", "app.bin"),
                svc.ResolveFirmware(new GamebuinoProject { Name = "Autre", FolderPath = dir }));
        }

        [Fact]
        public void ResolveFirmware_PlatformIO_FindsFirmwareBin()
        {
            var (svc, _, root) = Make(null);
            var dir = Proj(root, "Pio", p => {
                var e = Path.Combine(p, ".pio", "build", "aka"); Directory.CreateDirectory(e);
                File.WriteAllBytes(Path.Combine(e, "firmware.bin"), new byte[500]);
            });
            Assert.Equal(Path.Combine(dir, ".pio", "build", "aka", "firmware.bin"),
                svc.ResolveFirmware(new GamebuinoProject { Name = "Pio", FolderPath = dir }));
        }

        [Fact]
        public void ResolveFirmware_NotBuilt_ReturnsNull()
        {
            var (svc, _, root) = Make(null);
            var dir = Proj(root, "Vide", _ => { });
            Assert.Null(svc.ResolveFirmware(new GamebuinoProject { Name = "Vide", FolderPath = dir }));
        }

        [Fact]
        public async Task Launch_BuildsBootGameArguments_WithAbsolutePaths()
        {
            var emu = Path.Combine(Path.GetTempPath(), "aka-emu-" + Guid.NewGuid().ToString("N"), "run.sh");
            var (svc, runner, root) = Make(emu);
            try
            {
                var dir = Proj(root, "Jeu", p => {
                    var b = Path.Combine(p, "build"); Directory.CreateDirectory(b);
                    File.WriteAllBytes(Path.Combine(b, "Jeu.bin"), new byte[500]);
                    Directory.CreateDirectory(Path.Combine(p, "sdcard_files"));
                });
                await svc.LaunchAsync(new GamebuinoProject { Name = "Jeu", FolderPath = dir }, null);
                var req = runner.Last!;
                Assert.Equal(emu, req.FileName);
                Assert.Equal("--game", req.Arguments[0]);
                Assert.True(Path.IsPathRooted(req.Arguments[1]));
                Assert.Contains("--boot-game", req.Arguments);
                Assert.Contains("--sd", req.Arguments);
                Assert.Equal(Path.GetDirectoryName(emu), req.WorkingDirectory);
            }
            finally { try { Directory.Delete(Path.GetDirectoryName(emu)!, true); } catch { } }
        }

        [Fact]
        public async Task Launch_FirmwareNotBuilt_Throws()
        {
            var emu = Path.Combine(Path.GetTempPath(), "aka-emu-" + Guid.NewGuid().ToString("N"), "run.sh");
            var (svc, _, root) = Make(emu);
            try
            {
                var dir = Proj(root, "PasCompile", _ => { });
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => svc.LaunchAsync(new GamebuinoProject { Name = "PasCompile", FolderPath = dir }, null));
            }
            finally { try { Directory.Delete(Path.GetDirectoryName(emu)!, true); } catch { } }
        }

        [Fact]
        public async Task LaunchFirmware_EmulatorMissing_Throws()
        {
            var (svc, _, root) = Make(null);
            var fw = Path.Combine(root, "x.bin"); File.WriteAllBytes(fw, new byte[10]);
            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.LaunchFirmwareAsync(fw, null, null));
        }

        [Fact]
        public void IsAvailable_TrueWhenConfiguredFileExists()
        {
            var emu = Path.Combine(Path.GetTempPath(), "aka-emu-" + Guid.NewGuid().ToString("N"), "run.sh");
            var (svc, _, _) = Make(emu);
            try { Assert.True(svc.IsAvailable()); }
            finally { try { Directory.Delete(Path.GetDirectoryName(emu)!, true); } catch { } }
        }
    }
}
