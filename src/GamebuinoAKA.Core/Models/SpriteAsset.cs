using System;
using Newtonsoft.Json;

namespace GamebuinoAKA.Core.Models
{
    public class SpriteAsset
    {
        public string Name { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public int FrameCount { get; set; } = 1;
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public ColorFormat ColorFormat { get; set; } = ColorFormat.Bgr565Aka;
        public bool UseTransparency { get; set; } = true;
        public ushort TransparentKey { get; set; } = 0xF81F;
        public ushort[]? PixelData { get; set; }
        public double FrameDurationMs { get; set; } = 100;
    }
}
