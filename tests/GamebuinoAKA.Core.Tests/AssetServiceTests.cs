using System;
using System.IO;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using SkiaSharp;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class AssetServiceTests
    {
        private static AssetService New() => new(new SettingsService(new TempPlatformPaths()));
        private static SKBitmap Bmp(int w, int h) => new(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Unpremul));

        [Fact]
        public void Pack_Bgr565Aka_MatchesReferenceVectors()
        {
            Assert.Equal(0x001F, AssetService.Pack(255, 0, 0, ColorFormat.Bgr565Aka));
            Assert.Equal(0xF800, AssetService.Pack(0, 0, 255, ColorFormat.Bgr565Aka));
            Assert.Equal(0x07E0, AssetService.Pack(0, 255, 0, ColorFormat.Bgr565Aka));
            Assert.Equal(0xFFFF, AssetService.Pack(255, 255, 255, ColorFormat.Bgr565Aka));
            Assert.Equal(0x0000, AssetService.Pack(0, 0, 0, ColorFormat.Bgr565Aka));
        }

        [Fact]
        public void Pack_Rgb565Std_SwapsRedBlue()
        {
            Assert.Equal(0xF800, AssetService.Pack(255, 0, 0, ColorFormat.Rgb565Std));
            Assert.Equal(0x001F, AssetService.Pack(0, 0, 255, ColorFormat.Rgb565Std));
        }

        [Fact]
        public void Unpack_RoundTrips_PureColors()
        {
            Assert.Equal(((byte)255, (byte)0, (byte)0), AssetService.Unpack(0x001F, ColorFormat.Bgr565Aka));
            Assert.Equal(((byte)0, (byte)0, (byte)255), AssetService.Unpack(0xF800, ColorFormat.Bgr565Aka));
        }

        [Fact]
        public void PackBitmap_AlphaThreshold_UsesKeyBelow128()
        {
            using var b = Bmp(3, 1);
            b.SetPixel(0, 0, new SKColor(255, 0, 0, 255));
            b.SetPixel(1, 0, new SKColor(0, 0, 0, 127));
            b.SetPixel(2, 0, new SKColor(0, 0, 0, 128));
            var packed = AssetService.PackBitmap(b, ColorFormat.Bgr565Aka, 0xF81F);
            Assert.Equal(0x001F, packed[0]);
            Assert.Equal(0xF81F, packed[1]);
            Assert.Equal(0x0000, packed[2]);
        }

        [Fact]
        public void Crop_ReturnsSubregion()
        {
            var svc = New();
            using var b = Bmp(4, 4);
            b.SetPixel(1, 1, new SKColor(10, 20, 30, 255));
            using var c = svc.Crop(b, 1, 1, 2, 2);
            Assert.Equal(2, c.Width);
            var px = c.GetPixel(0, 0);
            Assert.Equal(10, px.Red); Assert.Equal(20, px.Green); Assert.Equal(30, px.Blue);
        }

        [Fact]
        public void Resize_ChangesDimensions()
        {
            var svc = New();
            using var b = Bmp(4, 4);
            using var s = svc.Resize(b, 2, 2, smooth: false);
            Assert.Equal(2, s.Width); Assert.Equal(2, s.Height);
        }

        [Fact]
        public void BuildSprite_PacksAllPixels()
        {
            var svc = New();
            using var b = Bmp(2, 2);
            var a = svc.BuildSprite(b, "t", ColorFormat.Bgr565Aka, 0xF81F, true);
            Assert.Equal(4, a.PixelData!.Length);
        }

        [Fact]
        public void SaveLoad_Gbspr_RoundTrips()
        {
            var svc = New();
            using var b = Bmp(2, 2);
            b.SetPixel(0, 0, new SKColor(255, 0, 0, 255));
            var a = svc.BuildSprite(b, "sp", ColorFormat.Bgr565Aka, 0xF81F, true);
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + AssetService.SpriteProjectExt);
            try { svc.SaveSprite(a, path); var r = svc.LoadSprite(path); Assert.Equal(a.PixelData!, r.PixelData!); }
            finally { File.Delete(path); }
        }

        [Fact]
        public void ExportSprite_IsDeterministic_AndContainsHex()
        {
            var svc = New();
            using var b = Bmp(1, 1);
            b.SetPixel(0, 0, new SKColor(255, 0, 0, 255));
            var a = svc.BuildSprite(b, "rouge", ColorFormat.Bgr565Aka, 0xF81F, false);
            var c1 = svc.ExportSpriteToCpp(a); var c2 = svc.ExportSpriteToCpp(a);
            Assert.Equal(c1, c2);
            Assert.Contains("0x001F", c1);
        }

        [Fact]
        public void ExportTilemap_ContainsLayersAndDims()
        {
            var svc = New();
            var t = new TilemapAsset { Name = "map", MapColumns = 2, MapRows = 2, BackgroundLayer = new byte[4], ForegroundLayer = new byte[4] };
            var code = svc.ExportTilemapToCpp(t);
            Assert.Contains("map_bg", code);
            Assert.Contains("map_fg", code);
        }

        [Fact]
        public void SaveLoad_Gbmap_RoundTrips()
        {
            var svc = New();
            var t = new TilemapAsset { Name = "m", MapColumns = 3, MapRows = 2, BackgroundLayer = new byte[6], ForegroundLayer = new byte[6] };
            t.BackgroundLayer[4] = 7;
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + AssetService.TilemapProjectExt);
            try { svc.SaveTilemap(t, path); var u = svc.LoadTilemap(path); Assert.Equal(t.BackgroundLayer, u.BackgroundLayer); }
            finally { File.Delete(path); }
        }
    }
}
