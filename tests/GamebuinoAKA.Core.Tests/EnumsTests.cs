using GamebuinoAKA.Core.Models;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class EnumsTests
    {
        [Fact] public void BuildSystem_HasStableNumericValues()
        { Assert.Equal(0, (int)BuildSystem.PlatformIO); Assert.Equal(1, (int)BuildSystem.EspIdf); }

        [Fact] public void ColorFormat_DefaultIsBgr565Aka()
        { Assert.Equal(ColorFormat.Bgr565Aka, new AppSettings().DefaultColorFormat); }
    }
}
