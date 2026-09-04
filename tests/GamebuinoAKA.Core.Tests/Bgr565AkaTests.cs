using GamebuinoAKA.Core.Graphics;

namespace GamebuinoAKA.Core.Tests;

public sealed class Bgr565AkaTests
{
    [Fact]
    public void Pack_Red_UsesLowBits()
    {
        Assert.Equal((ushort)0x001F, Bgr565Aka.Pack(255, 0, 0));
    }

    [Fact]
    public void Pack_Blue_UsesHighBits()
    {
        Assert.Equal((ushort)0xF800, Bgr565Aka.Pack(0, 0, 255));
    }

    [Fact]
    public void TransparentMagenta_HasExpectedValue()
    {
        Assert.Equal((ushort)0xF81F, Bgr565Aka.TransparentMagenta);
    }
}
