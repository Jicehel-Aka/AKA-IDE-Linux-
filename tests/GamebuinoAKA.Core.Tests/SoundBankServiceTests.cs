using System.IO;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class SoundBankServiceTests
    {
        private static (SoundBankService svc, string ws) Make(TempPlatformPaths paths)
        {
            var settings = new SettingsService(paths);
            var ws = Path.Combine(paths.Root, "ws"); Directory.CreateDirectory(ws);
            settings.Settings.WorkspaceFolder = ws;
            return (new SoundBankService(settings), ws);
        }

        [Fact] public void LoadBank_NoFile_ReturnsEmpty()
        { var (s, _) = Make(new TempPlatformPaths()); Assert.Empty(s.LoadBank().Assets); }

        [Fact]
        public void SaveLoad_Bank_RoundTrips()
        {
            var (s, _) = Make(new TempPlatformPaths());
            var bank = new SoundBank(); bank.Assets.Add(new SoundAsset { Name = "boom" });
            s.SaveBank(bank);
            Assert.Single(s.LoadBank().Assets);
        }

        [Fact]
        public void ImportFile_Wav_AddsSoundFx()
        {
            var (s, ws) = Make(new TempPlatformPaths());
            var wav = Path.Combine(ws, "bip.wav"); File.WriteAllBytes(wav, new byte[] { 0x52, 0x49, 0x46, 0x46 });
            var bank = new SoundBank();
            var a = s.ImportFile(wav, bank);
            Assert.Single(bank.Assets); Assert.Equal(SoundAssetType.SoundFx, a.AssetType);
        }

        [Fact]
        public async Task ScanProject_ClassifiesWavAndHeaders()
        {
            var (s, ws) = Make(new TempPlatformPaths());
            var proj = Path.Combine(ws, "jeu"); Directory.CreateDirectory(proj);
            File.WriteAllBytes(Path.Combine(proj, "saut.wav"), new byte[] { 0x52, 0x49, 0x46, 0x46 });
            File.WriteAllText(Path.Combine(proj, "theme.h"), "const uint8_t PMF_DATA[] = {0};");
            var found = await s.ScanProjectAsync(new GamebuinoProject { Name = "jeu", FolderPath = proj }, new SoundBank());
            Assert.Contains(found, a => a.FileExtension == ".wav");
        }

        [Fact]
        public void RemoveAsset_RemovesById()
        {
            var (s, _) = Make(new TempPlatformPaths());
            var bank = new SoundBank(); var a = new SoundAsset { Name = "x" }; bank.Assets.Add(a);
            s.RemoveAsset(a, bank);
            Assert.Empty(bank.Assets);
        }
    }
}
