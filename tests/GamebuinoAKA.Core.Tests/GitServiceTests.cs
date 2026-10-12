using System;
using System.IO;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class GitServiceTests
    {
        private static SettingsService WithWs(TempPlatformPaths paths, string ws)
        { var s = new SettingsService(paths); s.Settings.WorkspaceFolder = ws; return s; }

        private static (GitService git, RecordingProcessRunner runner) Make(TempPlatformPaths paths, string ws, string? gitPath = "git")
        {
            var runner = new RecordingProcessRunner { ExitToReturn = 0 };
            var tools = new FakeToolLocator();
            if (gitPath != null) tools.Map["git"] = gitPath;
            // simule un clone réussi : crée le dossier cible
            return (new GitService(WithWs(paths, ws), new CloningRunner(runner), tools), runner);
        }

        // Décore RecordingProcessRunner pour créer le dossier cible (arg #3) au clone.
        private sealed class CloningRunner : GamebuinoAKA.Core.Platform.IProcessRunner
        {
            private readonly RecordingProcessRunner _inner;
            public CloningRunner(RecordingProcessRunner inner) => _inner = inner;
            public Task<GamebuinoAKA.Core.Models.ProcessResult> RunAsync(GamebuinoAKA.Core.Platform.ProcessRequest r, System.Threading.CancellationToken ct = default) => _inner.RunAsync(r, ct);
            public Task<int> RunStreamingAsync(GamebuinoAKA.Core.Platform.ProcessRequest r, Action<string>? o, System.Threading.CancellationToken ct = default)
            {
                if (r.Arguments.Count >= 3) Directory.CreateDirectory(r.Arguments[2]);
                return _inner.RunStreamingAsync(r, o, ct);
            }
        }

        [Fact]
        public async Task Clone_BuildsSeparatedArguments_AndReturnsDestination()
        {
            var paths = new TempPlatformPaths();
            var ws = Path.Combine(paths.Root, "ws"); Directory.CreateDirectory(ws);
            var (git, runner) = Make(paths, ws);
            var dest = await git.CloneAsync("https://github.com/u/my-repo.git", null, null);
            Assert.Equal(Path.Combine(ws, "my-repo"), dest);
            Assert.Equal("git", runner.Last!.FileName);
            Assert.Equal(new[] { "clone", "https://github.com/u/my-repo.git", dest }, runner.Last.Arguments);
        }

        [Fact]
        public async Task Clone_PreservesSpacesInDestination()
        {
            var paths = new TempPlatformPaths();
            var ws = Path.Combine(paths.Root, "ws"); Directory.CreateDirectory(ws);
            var (git, _) = Make(paths, ws);
            var dest = await git.CloneAsync("https://github.com/u/x", "mon dossier", null);
            Assert.Equal(Path.Combine(ws, "mon dossier"), dest);
        }

        [Fact]
        public async Task Clone_NoWorkspace_Throws()
        {
            var paths = new TempPlatformPaths();
            var (git, _) = Make(paths, string.Empty);
            await Assert.ThrowsAsync<InvalidOperationException>(() => git.CloneAsync("https://github.com/u/x", null, null));
        }

        [Fact]
        public async Task Clone_DestinationExists_Throws()
        {
            var paths = new TempPlatformPaths();
            var ws = Path.Combine(paths.Root, "ws"); Directory.CreateDirectory(Path.Combine(ws, "deja"));
            var (git, _) = Make(paths, ws);
            await Assert.ThrowsAsync<InvalidOperationException>(() => git.CloneAsync("https://github.com/u/deja", "deja", null));
        }

        [Fact]
        public async Task Clone_GitNotFound_Throws()
        {
            var paths = new TempPlatformPaths();
            var ws = Path.Combine(paths.Root, "ws"); Directory.CreateDirectory(ws);
            var (git, _) = Make(paths, ws, gitPath: null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => git.CloneAsync("https://github.com/u/x", null, null));
        }

        [Theory]
        [InlineData("https://github.com/user/my-repo", "my-repo")]
        [InlineData("https://github.com/user/my-repo.git", "my-repo")]
        [InlineData("https://github.com/user/my-repo/", "my-repo")]
        [InlineData("git@github.com:user/other.git", "other")]
        public void ExtractRepoName_Works(string url, string expected)
            => Assert.Equal(expected, GitService.ExtractRepoName(url));
    }
}
