using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using Newtonsoft.Json;

namespace GamebuinoAKA.Core.Services
{
    public class SoundBankService : ISoundBankService
    {
        private static readonly string[] WavExtensions = { ".wav" };
        private static readonly string[] MusicExtensions = { ".pmf" };
        private static readonly string[] HeaderExtensions = { ".h" };
        private static readonly string[] FxHeaderKeywords = { "WAV_SYSTEM", "fx_system", "gb_audio_track_fx", "SOUND_FX" };
        private static readonly string[] MusicHeaderKeywords = { "pmf_data", "PMF_DATA", "gb_audio_track_pmf", "MUSIC_DATA" };

        private const string BankFileName = ".aka-soundbank.json";
        private readonly ISettingsService _settings;

        public SoundBankService(ISettingsService settings) => _settings = settings;

        public string BankFilePath => Path.Combine(_settings.Settings.WorkspaceFolder ?? string.Empty, BankFileName);

        public SoundBank LoadBank()
        {
            try
            {
                if (File.Exists(BankFilePath))
                {
                    var json = File.ReadAllText(BankFilePath);
                    return JsonConvert.DeserializeObject<SoundBank>(json) ?? new SoundBank();
                }
            }
            catch { }
            return new SoundBank();
        }

        public void SaveBank(SoundBank bank)
        {
            try { File.WriteAllText(BankFilePath, JsonConvert.SerializeObject(bank, Formatting.Indented)); }
            catch { }
        }

        public Task<List<SoundAsset>> ScanProjectAsync(GamebuinoProject project, SoundBank existingBank)
        {
            return Task.Run(() =>
            {
                var found = new List<SoundAsset>();
                if (!Directory.Exists(project.FolderPath)) return found;

                var knownPaths = new HashSet<string>(existingBank.Assets.Select(a => a.FilePath), StringComparer.OrdinalIgnoreCase);
                ScanDirectory(project.FolderPath, project.Name, knownPaths, found);
                return found;
            });
        }

        private static void ScanDirectory(string dir, string projectName, HashSet<string> knownPaths, List<SoundAsset> result)
        {
            var dirName = Path.GetFileName(dir);
            if (dirName is ".pio" or "build" or ".git" or "__pycache__" or "node_modules") return;

            foreach (var file in Directory.EnumerateFiles(dir))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (knownPaths.Contains(file)) continue;

                SoundAsset? asset = null;
                if (WavExtensions.Contains(ext)) asset = CreateFromWav(file, projectName);
                else if (MusicExtensions.Contains(ext)) asset = CreateFromPmf(file, projectName);
                else if (HeaderExtensions.Contains(ext)) asset = TryCreateFromHeader(file, projectName);

                if (asset != null) result.Add(asset);
            }

            foreach (var sub in Directory.EnumerateDirectories(dir))
                ScanDirectory(sub, projectName, knownPaths, result);
        }

        private static SoundAsset CreateFromWav(string path, string projectName)
        {
            var fi = new FileInfo(path);
            double duration = 0;
            try { duration = GetWavDuration(path); } catch { }

            return new SoundAsset
            {
                Name = Path.GetFileNameWithoutExtension(path),
                FilePath = path,
                FileExtension = ".wav",
                PreviewWavPath = path,
                AssetType = SoundAssetType.SoundFx,
                Theme = GuessTheme(path),
                SourceProject = projectName,
                FileSizeBytes = fi.Length,
                DurationSeconds = duration
            };
        }

        private static SoundAsset CreateFromPmf(string path, string projectName)
        {
            var fi = new FileInfo(path);
            return new SoundAsset
            {
                Name = Path.GetFileNameWithoutExtension(path),
                FilePath = path,
                FileExtension = ".pmf",
                PreviewWavPath = string.Empty,
                AssetType = SoundAssetType.Music,
                Theme = GuessTheme(path),
                SourceProject = projectName,
                FileSizeBytes = fi.Length
            };
        }

        private static SoundAsset? TryCreateFromHeader(string path, string projectName)
        {
            try
            {
                var content = File.ReadAllText(path);
                bool isFx = FxHeaderKeywords.Any(k => content.Contains(k, StringComparison.OrdinalIgnoreCase));
                bool isMusic = MusicHeaderKeywords.Any(k => content.Contains(k, StringComparison.OrdinalIgnoreCase));
                if (!isFx && !isMusic) return null;

                var fi = new FileInfo(path);
                return new SoundAsset
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    FilePath = path,
                    FileExtension = ".h",
                    PreviewWavPath = string.Empty,
                    AssetType = isMusic ? SoundAssetType.Music : SoundAssetType.SoundFx,
                    Theme = GuessTheme(path),
                    SourceProject = projectName,
                    FileSizeBytes = fi.Length
                };
            }
            catch { return null; }
        }

        private static string GuessTheme(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            var dir = Path.GetDirectoryName(path) ?? string.Empty;

            if (name.Contains("music") || name.Contains("track") || name.Contains("theme") || dir.Contains("music", StringComparison.OrdinalIgnoreCase))
                return "Musique";
            if (name.Contains("ui") || name.Contains("menu") || name.Contains("click") || name.Contains("button"))
                return "Interface";
            if (name.Contains("explode") || name.Contains("explo") || name.Contains("boom"))
                return "Explosions";
            if (name.Contains("laser") || name.Contains("shoot") || name.Contains("bullet"))
                return "Armes";
            if (name.Contains("jump") || name.Contains("land") || name.Contains("walk") || name.Contains("step"))
                return "Personnage";
            if (name.Contains("collect") || name.Contains("coin") || name.Contains("item") || name.Contains("pickup"))
                return "Collectibles";
            if (name.Contains("ambiance") || name.Contains("ambient") || name.Contains("wind") || name.Contains("rain"))
                return "Ambiance";
            return "Divers";
        }

        public SoundAsset ImportFile(string filePath, SoundBank bank)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            SoundAsset asset;
            if (WavExtensions.Contains(ext)) asset = CreateFromWav(filePath, string.Empty);
            else if (MusicExtensions.Contains(ext)) asset = CreateFromPmf(filePath, string.Empty);
            else
            {
                asset = new SoundAsset
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    FilePath = filePath,
                    FileExtension = ext,
                    AssetType = SoundAssetType.SoundFx,
                    Theme = "Divers"
                };
            }

            bank.Assets.Add(asset);
            SaveBank(bank);
            return asset;
        }

        public void RemoveAsset(SoundAsset asset, SoundBank bank)
        {
            bank.Assets.RemoveAll(a => a.Id == asset.Id);
            SaveBank(bank);
        }

        public void UpdateAsset(SoundAsset updated, SoundBank bank)
        {
            var idx = bank.Assets.FindIndex(a => a.Id == updated.Id);
            if (idx >= 0) bank.Assets[idx] = updated;
            SaveBank(bank);
        }

        public string AddToProject(SoundAsset asset, GamebuinoProject project)
        {
            string assetsDir = Path.Combine(project.IsPlatformIO ? project.SrcPath : project.MainPath, "assets");
            Directory.CreateDirectory(assetsDir);
            string dest = Path.Combine(assetsDir, Path.GetFileName(asset.FilePath));
            if (!File.Exists(dest)) File.Copy(asset.FilePath, dest);
            return dest;
        }

        private static double GetWavDuration(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs);
            br.ReadBytes(4); br.ReadInt32(); br.ReadBytes(4);
            br.ReadBytes(4);
            int fmtSize = br.ReadInt32();
            br.ReadInt16();
            short channels = br.ReadInt16();
            int sampleRate = br.ReadInt32();
            br.ReadInt32(); br.ReadInt16();
            short bitsPerSample = br.ReadInt16();
            if (fmtSize > 16) br.ReadBytes(fmtSize - 16);
            br.ReadBytes(4);
            int dataSize = br.ReadInt32();
            if (sampleRate <= 0 || channels <= 0 || bitsPerSample <= 0) return 0;
            double bytesPerSample = bitsPerSample / 8.0;
            return (dataSize / (bytesPerSample * channels)) / sampleRate;
        }
    }
}
