using System.IO;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class TemplateServiceTests
    {
        private static string Dest(TempPlatformPaths p)
        { var d = Path.Combine(p.Root, "dest"); Directory.CreateDirectory(d); return d; }

        [Fact]
        public async Task Create_PlatformIO_HelloWorld_WritesIniAndMain()
        {
            var paths = new TempPlatformPaths();
            var dest = Dest(paths);
            await new TemplateService(new SettingsService(paths))
                .CreateProjectAsync("MonJeu", "hello-world", dest, BuildSystem.PlatformIO);
            var dir = Path.Combine(dest, "MonJeu");
            Assert.True(File.Exists(Path.Combine(dir, "platformio.ini")));
            Assert.True(File.Exists(Path.Combine(dir, "src", "main.cpp")));
        }

        [Fact]
        public async Task Create_EspIdf_WritesCoquilleSkeleton_AndBuildMarker()
        {
            var paths = new TempPlatformPaths();
            var settings = new SettingsService(paths);
            settings.Settings.ReferenceGamebuinoComponentPath = string.Empty;
            var dest = Dest(paths);

            await new TemplateService(settings).CreateProjectAsync("MonIdf", "esp-idf", dest, BuildSystem.EspIdf);

            var dir = Path.Combine(dest, "MonIdf");
            Assert.True(File.Exists(Path.Combine(dir, "CMakeLists.txt")));
            Assert.True(File.Exists(Path.Combine(dir, "sdkconfig.defaults")));
            Assert.True(File.Exists(Path.Combine(dir, "partitions.csv")));
            Assert.True(File.Exists(Path.Combine(dir, "main", "app_main.cpp")));
            Assert.True(File.Exists(Path.Combine(dir, "main", "game_module.h")));
            var marker = Path.Combine(dir, GamebuinoProject.BuildMarkerFile);
            Assert.True(File.Exists(marker));
            Assert.Contains("espidf", File.ReadAllText(marker));
            Assert.True(File.Exists(Path.Combine(dir, "components", "gamebuino", "AJOUTER_LA_LIB.md")));
            Assert.Equal(BuildSystem.EspIdf, GamebuinoProject.DetectBuildSystem(dir, BuildSystem.PlatformIO));
        }

        [Fact]
        public async Task Create_EspIdf_WithAudio_WritesAudioModule_AndWiresIt()
        {
            var paths = new TempPlatformPaths();
            var dest = Dest(paths);
            await new TemplateService(new SettingsService(paths))
                .CreateProjectAsync("Sonore", "esp-idf", dest, BuildSystem.EspIdf, withAudio: true);

            var main = Path.Combine(dest, "Sonore", "main");
            Assert.True(File.Exists(Path.Combine(main, "audio.h")));
            Assert.True(File.Exists(Path.Combine(main, "audio.cpp")));
            Assert.Contains("audio.cpp", File.ReadAllText(Path.Combine(main, "CMakeLists.txt")));
            var appMain = File.ReadAllText(Path.Combine(main, "app_main.cpp"));
            Assert.Contains("audio_init", appMain);
            Assert.Contains("audio.h", appMain);
        }

        [Fact]
        public async Task Create_EspIdf_WithoutAudio_HasNoAudioModule()
        {
            var paths = new TempPlatformPaths();
            var dest = Dest(paths);
            await new TemplateService(new SettingsService(paths))
                .CreateProjectAsync("Muet", "esp-idf", dest, BuildSystem.EspIdf, withAudio: false);

            var main = Path.Combine(dest, "Muet", "main");
            Assert.False(File.Exists(Path.Combine(main, "audio.cpp")));
            Assert.DoesNotContain("audio.cpp", File.ReadAllText(Path.Combine(main, "CMakeLists.txt")));
        }
    }
}
