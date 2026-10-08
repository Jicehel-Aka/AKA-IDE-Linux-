using System;

namespace GamebuinoAKA.Core.Platform
{
    public sealed class ApplicationLauncher : IApplicationLauncher
    {
        private readonly IProcessRunner _runner;
        public ApplicationLauncher(IProcessRunner runner) => _runner = runner;

        public void OpenFolder(string folderPath) => Open(folderPath);
        public void OpenFile(string filePath) => Open(filePath);
        public void OpenUrl(string url) => Open(url);

        private void Open(string target)
        {
            var request = OperatingSystem.IsWindows()
                ? new ProcessRequest("explorer.exe", new[] { target })
                : new ProcessRequest("xdg-open", new[] { target });
            _ = _runner.RunStreamingAsync(request, null);
        }
    }
}
