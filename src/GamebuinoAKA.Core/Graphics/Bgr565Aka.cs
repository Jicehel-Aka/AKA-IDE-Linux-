namespace GamebuinoAKA.Core.Graphics;

/// <summary>
/// Format BGR565 utilisé par Gamebuino AKA.
/// Rouge dans les bits faibles, bleu dans les bits forts.
/// </summary>
public static class Bgr565Aka
{
    public const ushort TransparentMagenta = 0xF81F;

    public static ushort Pack(byte red, byte green, byte blue)
    {
        return (ushort)(
            (red >> 3) |
            ((green >> 2) << 5) |
            ((blue >> 3) << 11));
    }
}
