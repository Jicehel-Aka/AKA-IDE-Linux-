using System.Collections.Generic;

namespace GamebuinoAKA.Core.Models
{
    public class AppSettings
    {
        public string WorkspaceFolder { get; set; } = string.Empty;
        public string PlatformIOPath { get; set; } = string.Empty;
        public string GamebuinoLibRepoUrl { get; set; } = "https://github.com/jmp42/Gamebuino_AKA_lib";
        public string IdfPyPath { get; set; } = string.Empty;
        public string IdfExportScript { get; set; } = string.Empty;
        public string IdfSerialPort { get; set; } = string.Empty;
        public string ReferenceGamebuinoComponentPath { get; set; } = string.Empty;
        public string VSCodePath { get; set; } = string.Empty;
        public string Theme { get; set; } = "Dark";
        public BuildSystem DefaultBuildSystem { get; set; } = BuildSystem.EspIdf;
        public ColorFormat DefaultColorFormat { get; set; } = ColorFormat.Bgr565Aka;
        public ushort DefaultTransparentKey { get; set; } = 0xF81F;
        public List<string> RecentProjects { get; set; } = new List<string>();
        public int MaxRecentProjects { get; set; } = 10;
        public bool AutoDetectTools { get; set; } = true;
    }
}
