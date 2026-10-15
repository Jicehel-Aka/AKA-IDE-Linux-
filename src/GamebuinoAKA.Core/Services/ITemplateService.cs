using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    public interface ITemplateService
    {
        Task CreateProjectAsync(string projectName, string template, string destinationFolder, BuildSystem buildSystem, bool withAudio = false);

        /// <summary>
        /// Crée un projet script MicroPython prêt à tourner : un main.py d'exemple
        /// plus le cadre (game8266.py / upygame.py / urandom.py / umachine.py /
        /// sprite.py). Exécutable tel quel via « Tester (PC) » (aka-py-sim).
        /// </summary>
        Task CreateMicroPythonProjectAsync(string projectName, string destinationFolder);
    }
}
