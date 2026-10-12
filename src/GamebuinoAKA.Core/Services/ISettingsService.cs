using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    public interface ISettingsService
    {
        AppSettings Settings { get; }
        string SettingsFilePath { get; }
        void Load();
        void Save();
        void AddRecentProject(string folderPath);
    }
}
