using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Platform;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class ProcessRunnerTests
    {
        [Fact]
        public async Task RunAsync_Dotnet_Version_Succeeds()
        {
            var r = await new SystemProcessRunner().RunAsync(new ProcessRequest("dotnet", new[] { "--version" }));
            Assert.Equal(0, r.ExitCode);
            Assert.True(r.Success);
            Assert.False(string.IsNullOrWhiteSpace(r.StandardOutput));
        }

        [Fact]
        public async Task RunStreamingAsync_StreamsOutput()
        {
            var lines = new List<string>();
            var exit = await new SystemProcessRunner().RunStreamingAsync(
                new ProcessRequest("dotnet", new[] { "--version" }), l => { lock (lines) lines.Add(l); });
            Assert.Equal(0, exit);
            Assert.NotEmpty(lines);
        }

        [Fact]
        public async Task RunAsync_MissingExecutable_Throws()
            => await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new SystemProcessRunner().RunAsync(new ProcessRequest("aka-outil-inexistant-xyz", Array.Empty<string>())));
    }

    public class ToolLocatorTests
    {
        [Fact] public void Locate_OnPath_ReturnsPath() => Assert.NotNull(new ToolLocator().Locate("dotnet"));
        [Fact] public void Locate_Unknown_ReturnsNull() => Assert.Null(new ToolLocator().Locate("aka-outil-inexistant-xyz"));

        [Fact]
        public void Locate_ConfiguredPath_TakesPriority()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "aka-fake-" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(tmp, "#!/bin/sh\n");
            try { Assert.Equal(tmp, new ToolLocator().Locate("git", tmp)); }
            finally { File.Delete(tmp); }
        }

        [Fact]
        public void Locate_ConfiguredPathMissing_FallsBackToPath()
            => Assert.NotNull(new ToolLocator().Locate("dotnet",
                Path.Combine(Path.GetTempPath(), "n-existe-pas-" + Guid.NewGuid().ToString("N"))));
    }
}
