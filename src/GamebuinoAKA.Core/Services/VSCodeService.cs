using System;
using System.IO;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Platform;

namespace GamebuinoAKA.Core.Services
{
    public sealed class VSCodeService : IVSCodeService
    {
        private readonly ISettingsService _settings;
        private readonly IProcessRunner _runner;
        private readonly IToolLocator _tools;

        public VSCodeService(ISettingsService settings, IProcessRunner runner, IToolLocator tools)
        {
            _settings = settings;
            _runner = runner;
            _tools = tools;
        }

        public string DetectVSCodePath() => _tools.Locate("code", _settings.Settings.VSCodePath) ?? string.Empty;
        public bool IsInstalled() => !string.IsNullOrEmpty(DetectVSCodePath());
        public void OpenProject(GamebuinoProject project) => OpenFolder(project.FolderPath);
        public void OpenFolder(string folderPath)
        {
            var code = DetectVSCodePath();
            if (string.IsNullOrEmpty(code)) throw new InvalidOperationException("VS Code introuvable.");
            _ = _runner.RunStreamingAsync(new ProcessRequest(code, new[] { folderPath }), null);
        }
        public string GetDisplayPath() => Path.GetFileName(DetectVSCodePath());
    }
}
