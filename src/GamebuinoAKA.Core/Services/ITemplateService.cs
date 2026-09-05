using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    public interface ITemplateService
    {
        /// <summary>
        /// Crée un projet dans destinationFolder/projectName selon la chaîne de build
        /// (PlatformIO Arduino, ou ESP-IDF composants CMake + coquille).
        /// </summary>
        Task CreateProjectAsync(string projectName, string template,
            string destinationFolder, BuildSystem buildSystem);
    }
}
