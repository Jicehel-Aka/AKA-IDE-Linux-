using System;
using Newtonsoft.Json;

namespace GamebuinoAKA.Core.Models
{
    public class TilemapAsset
    {
        public string Name { get; set; } = string.Empty;
        public string TilesetPath { get; set; } = string.Empty;
        public int TileWidth { get; set; } = 16;
        public int TileHeight { get; set; } = 16;
        public int MapColumns { get; set; } = 20;
        public int MapRows { get; set; } = 15;
        public int TotalTiles => MapColumns * MapRows;
        public byte[] BackgroundLayer { get; set; } = Array.Empty<byte>();
        public byte[] ForegroundLayer { get; set; } = Array.Empty<byte>();
        public ushort[]? TilesetPixels { get; set; }
        public int TilesetColumns { get; set; }
        public int TilesetRows { get; set; }
        public ColorFormat ColorFormat { get; set; } = ColorFormat.Bgr565Aka;
        public bool UseTransparency { get; set; } = true;
        public ushort TransparentKey { get; set; } = 0xF81F;
        public void InitializeLayers()
        {
            BackgroundLayer = new byte[MapColumns * MapRows];
            ForegroundLayer = new byte[MapColumns * MapRows];
        }
    }
}
