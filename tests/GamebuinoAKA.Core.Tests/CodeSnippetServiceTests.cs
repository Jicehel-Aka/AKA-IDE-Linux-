using System.IO;
using System.Linq;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class CodeSnippetServiceTests
    {
        private static (CodeSnippetService svc, SettingsService settings) Make()
        {
            var paths = new TempPlatformPaths();
            var settings = new SettingsService(paths);
            settings.Settings.WorkspaceFolder = Path.Combine(paths.Root, "ws");
            Directory.CreateDirectory(settings.Settings.WorkspaceFolder);
            return (new CodeSnippetService(settings), settings);
        }

        [Fact]
        public void GetAll_PlatformIO_OnlyPlatformIO()
        {
            var (svc, _) = Make();
            var list = svc.GetAll(BuildSystem.PlatformIO);
            Assert.NotEmpty(list);
            Assert.All(list, s => Assert.True(s.ForPlatformIO));
        }

        [Fact]
        public void GetAll_EspIdf_OnlyEspIdf_AndContainsShapes()
        {
            var (svc, _) = Make();
            var list = svc.GetAll(BuildSystem.EspIdf);
            Assert.NotEmpty(list);
            Assert.All(list, s => Assert.True(s.ForEspIdf));
            Assert.Contains(list, s => s.Id == "esp_gfx_shapes");
        }

        [Fact]
        public void GenerateFiles_EspIdf_InjectsIntoAppMain()
        {
            var (svc, _) = Make();
            var snip = svc.GetAll(BuildSystem.EspIdf).First(s => s.Id == "esp_gfx_shapes");
            var files = svc.GenerateFiles(new[] { snip }, BuildSystem.EspIdf, "MonJeu");
            Assert.Contains(files.Keys, k => k.Contains("app_main"));
            var appMain = files.First(kv => kv.Key.Contains("app_main")).Value;
            Assert.Contains("fillRect", appMain);
        }

        [Fact]
        public void GenerateFiles_PlatformIO_ProducesFiles()
        {
            var (svc, _) = Make();
            var snip = svc.GetAll(BuildSystem.PlatformIO).First();
            var files = svc.GenerateFiles(new[] { snip }, BuildSystem.PlatformIO, "MonJeu");
            Assert.NotEmpty(files);
        }

        [Fact]
        public void UserBank_SaveLoad_RoundTrips()
        {
            var (svc, settings) = Make();
            var bank = svc.LoadUserBank();
            bank.UserSnippets.Add(new CodeSnippet { Id = "u1", Name = "Mon snippet", ForPlatformIO = true });
            svc.SaveUserBank(bank);

            var reloaded = new CodeSnippetService(settings).LoadUserBank();
            Assert.Contains(reloaded.UserSnippets, s => s.Id == "u1");
        }
    }
}
