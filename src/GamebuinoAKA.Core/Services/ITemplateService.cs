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

        /// <summary>
        /// Crée un projet MicroPython à partir du source d'un exemple de la galerie :
        /// écrit <paramref name="mainPyContent"/> comme main.py et copie le cadre
        /// embarqué. Exécutable tel quel via « Tester (PC) ».
        /// </summary>
        Task CreateMicroPythonProjectFromContentAsync(string projectName, string destinationFolder, string mainPyContent);
    }
}
