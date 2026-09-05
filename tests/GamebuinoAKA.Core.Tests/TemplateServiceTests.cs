using System.IO;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class TemplateServiceTests
    {
        [Fact]
        public async Task Create_PlatformIO_HelloWorld_WritesIniAndMain()
        {
            using var paths = new TempPlatformPaths();
            var settings = new SettingsService(paths);
            var dest = Path.Combine(paths.Root, "dest");
            Directory.CreateDirectory(dest);

            var svc = new TemplateService(settings);
            await svc.CreateProjectAsync("MonJeu", "hello-world", dest, BuildSystem.PlatformIO);

            var projDir = Path.Combine(dest, "MonJeu");
            Assert.True(File.Exists(Path.Combine(projDir, "platformio.ini")));
            Assert.True(File.Exists(Path.Combine(projDir, "src", "main.cpp")));
        }

        [Fact]
        public async Task Create_EspIdf_WritesCoquilleSkeleton_AndBuildMarker()
        {
            using var paths = new TempPlatformPaths();
            var settings = new SettingsService(paths);
            // Pas de composant de référence configuré → un placeholder est écrit.
            settings.Settings.ReferenceGamebuinoComponentPath = string.Empty;

            var dest = Path.Combine(paths.Root, "dest");
            Directory.CreateDirectory(dest);

            var svc = new TemplateService(settings);
            await svc.CreateProjectAsync("MonIdf", "esp-idf", dest, BuildSystem.EspIdf);

            var projDir = Path.Combine(dest, "MonIdf");
            Assert.True(File.Exists(Path.Combine(projDir, "CMakeLists.txt")));
            Assert.True(File.Exists(Path.Combine(projDir, "sdkconfig.defaults")));
            Assert.True(File.Exists(Path.Combine(projDir, "partitions.csv")));
            Assert.True(File.Exists(Path.Combine(projDir, "main", "app_main.cpp")));
            Assert.True(File.Exists(Path.Combine(projDir, "main", "CMakeLists.txt")));

            // Marqueur de chaîne figé sur ESP-IDF
            var marker = Path.Combine(projDir, GamebuinoProject.BuildMarkerFile);
            Assert.True(File.Exists(marker));
            Assert.Contains("espidf", File.ReadAllText(marker));

            // Lib non fournie → note explicative
            Assert.True(File.Exists(Path.Combine(projDir, "components", "gamebuino", "AJOUTER_LA_LIB.md")));

            // Le dossier généré est bien reconnu comme ESP-IDF
            Assert.Equal(BuildSystem.EspIdf,
                GamebuinoProject.DetectBuildSystem(projDir, BuildSystem.PlatformIO));
        }
    }
}
