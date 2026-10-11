using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class ProjectServiceTests
    {
        private static (SettingsService settings, string ws) NewWorkspace(TempPlatformPaths paths)
        {
            var settings = new SettingsService(paths);
            var ws = Path.Combine(paths.Root, "ws");
            Directory.CreateDirectory(ws);
            settings.Settings.WorkspaceFolder = ws;
            return (settings, ws);
        }

        [Fact]
        public async Task ScanWorkspace_DetectsPioAndIdf_IgnoresOthers()
        {
            var paths = new TempPlatformPaths();
            var (settings, ws) = NewWorkspace(paths);

            var pio = Path.Combine(ws, "proj_pio");
            Directory.CreateDirectory(Path.Combine(pio, "src"));
            File.WriteAllText(Path.Combine(pio, "platformio.ini"), "[env:aka]\n");

            var idf = Path.Combine(ws, "proj_idf");
            Directory.CreateDirectory(Path.Combine(idf, "main"));
            File.WriteAllText(Path.Combine(idf, "CMakeLists.txt"), "include($ENV{IDF_PATH}/tools/cmake/project.cmake)\n");

            Directory.CreateDirectory(Path.Combine(ws, "rien"));

            var projects = await new ProjectService(settings).ScanWorkspaceAsync();
            Assert.Equal(2, projects.Count);
            Assert.Equal(BuildSystem.PlatformIO, projects.Single(x => x.Name == "proj_pio").BuildSystem);
            Assert.Equal(BuildSystem.EspIdf, projects.Single(x => x.Name == "proj_idf").BuildSystem);
        }

        [Fact]
        public async Task ScanWorkspace_EmptyOrMissing_ReturnsEmpty()
        {
            var paths = new TempPlatformPaths();
            var settings = new SettingsService(paths);
            settings.Settings.WorkspaceFolder = Path.Combine(paths.Root, "inexistant");
            Assert.Empty(await new ProjectService(settings).ScanWorkspaceAsync());
        }

        [Fact]
        public void DeleteProject_RemovesFolderAndRecentEntry()
        {
            var paths = new TempPlatformPaths();
            var (settings, ws) = NewWorkspace(paths);
            var dir = Path.Combine(ws, "todelete");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "platformio.ini"), "x");
            settings.Settings.RecentProjects.Add(dir);

            new ProjectService(settings).DeleteProject(new GamebuinoProject { Name = "todelete", FolderPath = dir });
            Assert.False(Directory.Exists(dir));
            Assert.DoesNotContain(dir, settings.Settings.RecentProjects);
        }
    }
}
