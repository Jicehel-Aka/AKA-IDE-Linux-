using System;
using System.Collections.Generic;
using System.IO;

namespace GamebuinoAKA.Core.Models
{
    public class GamebuinoProject
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string Template { get; set; } = "empty";
        public BuildSystem BuildSystem { get; set; } = BuildSystem.PlatformIO;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public List<string> RecentFiles { get; set; } = new List<string>();

        public string PlatformIniPath => Path.Combine(FolderPath, "platformio.ini");
        public string CMakeListsPath => Path.Combine(FolderPath, "CMakeLists.txt");
        public string SrcPath => Path.Combine(FolderPath, "src");
        public string MainPath => Path.Combine(FolderPath, "main");
        public bool IsPlatformIO => BuildSystem == BuildSystem.PlatformIO;
        public bool IsEspIdf => BuildSystem == BuildSystem.EspIdf;
        public string BuildSystemLabel => IsEspIdf ? "ESP-IDF" : "PlatformIO";
        public bool IsValid => !string.IsNullOrEmpty(FolderPath) && (File.Exists(PlatformIniPath) || File.Exists(CMakeListsPath));

        public const string BuildMarkerFile = ".aka-build";

        public static BuildSystem DetectBuildSystem(string folder, BuildSystem fallback = BuildSystem.PlatformIO)
        {
            try
            {
                var marker = Path.Combine(folder, BuildMarkerFile);
                if (File.Exists(marker))
                {
                    var v = File.ReadAllText(marker).Trim().ToLowerInvariant();
                    if (v.Contains("idf")) return BuildSystem.EspIdf;
                    if (v.Contains("pio") || v.Contains("platformio")) return BuildSystem.PlatformIO;
                }
            }
            catch { }

            if (File.Exists(Path.Combine(folder, "platformio.ini")))
                return BuildSystem.PlatformIO;

            var cmake = Path.Combine(folder, "CMakeLists.txt");
            if (File.Exists(cmake))
            {
                try
                {
                    var txt = File.ReadAllText(cmake);
                    if (txt.Contains("project.cmake") || txt.Contains("idf_component_register") ||
                        Directory.Exists(Path.Combine(folder, "main")))
                        return BuildSystem.EspIdf;
                }
                catch { }
            }

            return fallback;
        }

        public void WriteBuildMarker()
        {
            try { File.WriteAllText(Path.Combine(FolderPath, BuildMarkerFile), IsEspIdf ? "espidf" : "platformio"); }
            catch { }
        }
    }
}
