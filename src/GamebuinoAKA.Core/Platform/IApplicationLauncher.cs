namespace GamebuinoAKA.Core.Platform
{
    public interface IApplicationLauncher
    {
        void OpenFolder(string folderPath);
        void OpenFile(string filePath);
        void OpenUrl(string url);
    }
}
