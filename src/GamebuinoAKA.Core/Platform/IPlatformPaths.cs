namespace GamebuinoAKA.Core.Platform;

public interface IPlatformPaths
{
    string ConfigDirectory { get; }
    string DataDirectory { get; }
    string CacheDirectory { get; }
}
