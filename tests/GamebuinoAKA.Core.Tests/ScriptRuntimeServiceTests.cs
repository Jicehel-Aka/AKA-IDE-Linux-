using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class ScriptRuntimeServiceTests
    {
        private static (ScriptRuntimeService svc, SettingsService settings, RecordingProcessRunner runner, string root) Make()
        {
            var paths = new TempPlatformPaths();
            var settings = new SettingsService(paths);
            var runner = new RecordingProcessRunner();
            var tools = new FakeToolLocator { };
            tools.Map["python3"] = "python3";
            return (new ScriptRuntimeService(settings, runner, tools), settings, runner, paths.Root);
        }

        private static string Proj(string root, string name, Action<string> populate)
        { var d = Path.Combine(root, name); Directory.CreateDirectory(d); populate(d); return d; }

        // ── Détection ────────────────────────────────────────────────────────────
        [Fact]
        public void DetectKind_Love2D_FromMainLua()
        {
            var (svc, _, _, root) = Make();
            var dir = Proj(root, "jeu_love", d => File.WriteAllText(Path.Combine(d, "main.lua"), "function love.draw() end"));
            Assert.Equal(ProjectKind.Love2D, svc.DetectKind(dir));
        }

        [Fact]
        public void DetectKind_MicroPython_FromMainPy()
        {
            var (svc, _, _, root) = Make();
            var dir = Proj(root, "jeu_py", d => File.WriteAllText(Path.Combine(d, "main.py"), "import aka"));
            Assert.Equal(ProjectKind.MicroPython, svc.DetectKind(dir));
        }

        [Fact]
        public void DetectKind_EspIdf_FromMarker()
        {
            var (svc, _, _, root) = Make();
            var dir = Proj(root, "natif", d => {
                File.WriteAllText(Path.Combine(d, ".aka-build"), "espidf");
                File.WriteAllText(Path.Combine(d, "CMakeLists.txt"), "project(x)");
            });
            Assert.Equal(ProjectKind.EspIdfNative, svc.DetectKind(dir));
        }

        [Fact]
        public void DetectKind_PlatformIO_FromIni()
        {
            var (svc, _, _, root) = Make();
            var dir = Proj(root, "pio", d => File.WriteAllText(Path.Combine(d, "platformio.ini"), "[env]"));
            Assert.Equal(ProjectKind.PlatformIO, svc.DetectKind(dir));
        }

        [Fact]
        public void IsScript_OnlyForPyAndLua()
        {
            var (svc, _, _, _) = Make();
            Assert.True(svc.IsScript(ProjectKind.MicroPython));
            Assert.True(svc.IsScript(ProjectKind.Love2D));
            Assert.False(svc.IsScript(ProjectKind.EspIdfNative));
            Assert.False(svc.IsScript(ProjectKind.PlatformIO));
        }

        // ── Simulateur ─────────────────────────────────────────────────────────────
        [Fact]
        public void DetectSimPath_UsesConfiguredOverride()
        {
            var (svc, settings, _, root) = Make();
            var fake = Path.Combine(root, "mon-love-sim");
            File.WriteAllText(fake, "#!/bin/sh");
            settings.Settings.LoveSimPath = fake;
            Assert.Equal(fake, svc.DetectSimPath(ProjectKind.Love2D));
        }

        [Fact]
        public void DetectSimPath_NoSim_ReturnsEmpty()
        {
            var (svc, _, _, _) = Make();
            Assert.Equal(string.Empty, svc.DetectSimPath(ProjectKind.MicroPython));
        }

        [Fact]
        public async Task RunOnPc_Love2D_LaunchesSim_WithProjectDir()
        {
            var (svc, settings, runner, root) = Make();
            var sim = Path.Combine(root, "akalove-play"); File.WriteAllText(sim, "#!/bin/sh");
            settings.Settings.LoveSimPath = sim;
            var dir = Proj(root, "jeu", d => File.WriteAllText(Path.Combine(d, "main.lua"), "x"));

            await svc.RunOnPcAsync(dir, ProjectKind.Love2D, null);

            Assert.Equal(sim, runner.Last!.FileName);
            Assert.Equal(Path.GetFullPath(dir), runner.Last.Arguments[0]);
        }

        [Fact]
        public async Task RunOnPc_MicroPython_LaunchesSim_WithEntryScript()
        {
            var (svc, settings, runner, root) = Make();
            var sim = Path.Combine(root, "aka-py-sim"); File.WriteAllText(sim, "#!/bin/sh");
            settings.Settings.MicroPythonSimPath = sim;
            var dir = Proj(root, "jeu", d => File.WriteAllText(Path.Combine(d, "main.py"), "import aka"));

            await svc.RunOnPcAsync(dir, ProjectKind.MicroPython, null);

            Assert.Equal(sim, runner.Last!.FileName);
            Assert.EndsWith("main.py", runner.Last.Arguments[0]);
        }

        [Fact]
        public async Task RunOnPc_NoSim_Throws()
        {
            var (svc, _, _, root) = Make();
            var dir = Proj(root, "jeu", d => File.WriteAllText(Path.Combine(d, "main.lua"), "x"));
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.RunOnPcAsync(dir, ProjectKind.Love2D, null));
        }

        [Fact]
        public async Task RunOnPc_NativeProject_Throws()
        {
            var (svc, _, _, root) = Make();
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.RunOnPcAsync(root, ProjectKind.EspIdfNative, null));
        }

        // ── Envoi série ─────────────────────────────────────────────────────────────
        [Fact]
        public async Task SendToDevice_BuildsAkaTransferArgs()
        {
            var (svc, settings, runner, root) = Make();
            var transfer = Path.Combine(root, "akatransfer.py"); File.WriteAllText(transfer, "# tool");
            settings.Settings.AkaTransferPath = transfer;
            var script = Path.Combine(root, "main.py"); File.WriteAllText(script, "import aka");

            await svc.SendToDeviceAsync(script, ProjectKind.MicroPython, "/dev/ttyUSB0", null);

            var a = runner.Last!.Arguments;
            Assert.Equal("python3", runner.Last.FileName);
            Assert.Contains(transfer, a);
            Assert.Contains("--port", a); Assert.Contains("/dev/ttyUSB0", a);
            Assert.Contains("--lang", a); Assert.Contains("py", a);
            Assert.Contains("--dest", a); Assert.Contains("py/main.py", a);
        }

        [Fact]
        public async Task SendToDevice_NoPort_Throws()
        {
            var (svc, settings, _, root) = Make();
            var transfer = Path.Combine(root, "akatransfer.py"); File.WriteAllText(transfer, "# tool");
            settings.Settings.AkaTransferPath = transfer;
            var script = Path.Combine(root, "main.py"); File.WriteAllText(script, "x");
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.SendToDeviceAsync(script, ProjectKind.MicroPython, null, null));
        }

        [Fact]
        public async Task SendToDevice_Love2D_UsesLuaLangAndGamesDest()
        {
            var (svc, settings, runner, root) = Make();
            var transfer = Path.Combine(root, "akatransfer.py"); File.WriteAllText(transfer, "# tool");
            settings.Settings.AkaTransferPath = transfer;
            var script = Path.Combine(root, "main.lua"); File.WriteAllText(script, "x");

            await svc.SendToDeviceAsync(script, ProjectKind.Love2D, "COM5", null);

            var a = runner.Last!.Arguments;
            Assert.Contains("lua", a);
            Assert.Contains("games/main.lua", a);
        }
    }
}
