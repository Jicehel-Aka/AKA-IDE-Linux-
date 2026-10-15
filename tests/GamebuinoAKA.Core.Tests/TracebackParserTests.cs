using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class TracebackParserTests
    {
        [Theory]
        [InlineData("  File \"pong.py\", line 42, in update", "pong.py", 42)]
        [InlineData("File \"main.py\", line 7", "main.py", 7)]
        [InlineData("  File \"<stdin>\", line 3, in <module>", "<stdin>", 3)]
        [InlineData("File \"py/jeu.py\", line 128, in <module>", "py/jeu.py", 128)]
        public void ParseLine_ExtractsFileAndLine(string line, string file, int ln)
        {
            var loc = TracebackParser.ParseLine(line);
            Assert.NotNull(loc);
            Assert.Equal(file, loc!.File);
            Assert.Equal(ln, loc.Line);
        }

        [Theory]
        [InlineData("NameError: name 'x' is not defined")]
        [InlineData("Traceback (most recent call last):")]
        [InlineData("[Sim PC] Fermé (exit 0).")]
        [InlineData("")]
        [InlineData(null)]
        public void ParseLine_ReturnsNull_WhenNoLocation(string? line)
        {
            Assert.Null(TracebackParser.ParseLine(line));
        }
    }
}
