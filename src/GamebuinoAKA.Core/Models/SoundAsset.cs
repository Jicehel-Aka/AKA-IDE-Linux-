using System;
using System.Collections.Generic;

namespace GamebuinoAKA.Core.Models
{
    public enum SoundAssetType { SoundFx, Music }

    public class SoundAsset
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SoundAssetType AssetType { get; set; } = SoundAssetType.SoundFx;
        public string Theme { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public string FilePath { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public string PreviewWavPath { get; set; } = string.Empty;
        public string SourceProject { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public double DurationSeconds { get; set; }
        public long FileSizeBytes { get; set; }
        public string TypeLabel => AssetType == SoundAssetType.Music ? "Musique" : "Son FX";
        public string TypeIcon => AssetType == SoundAssetType.Music ? "🎵" : "🔊";
    }
}
