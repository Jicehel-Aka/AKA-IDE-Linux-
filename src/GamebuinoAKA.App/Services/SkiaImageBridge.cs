using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace GamebuinoAKA.App.Services
{
    public static class SkiaImageBridge
    {
        public static Bitmap ToAvalonia(SKBitmap bmp)
        {
            using var image = SKImage.FromBitmap(bmp);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Position = 0;
            return new Bitmap(ms);
        }
    }
}
