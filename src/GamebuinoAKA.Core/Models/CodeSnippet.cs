using System.Collections.Generic;

namespace GamebuinoAKA.Core.Models
{
    public enum SnippetTargetFile
    {
        MainCpp,
        GameCpp,
        GameH,
        NewHeader,
        NewCpp
    }

    public class CodeSnippet
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public bool ForPlatformIO { get; set; } = true;
        public bool ForEspIdf { get; set; } = false;
        public SnippetTargetFile TargetFile { get; set; } = SnippetTargetFile.GameCpp;
        public string Code { get; set; } = string.Empty;
        public List<string> RequiresSnippetIds { get; set; } = new();
        public bool IsUserDefined { get; set; } = false;
        public string CompatibilityLabel => (ForPlatformIO && ForEspIdf) ? "PIO + IDF" : ForPlatformIO ? "PlatformIO" : ForEspIdf ? "ESP-IDF" : "—";
    }

    public class SnippetBank
    {
        public int Version { get; set; } = 1;
        public List<CodeSnippet> UserSnippets { get; set; } = new();
    }
}
