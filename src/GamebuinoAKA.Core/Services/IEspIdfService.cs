using System.Threading.Tasks;

namespace GamebuinoAKA.Core.Services
{
    public interface IEspIdfService : IBuildBackend
    {
        Task<string> GetVersionAsync();
        string[] DetectSerialPorts();
        /// <summary>Détecte le script d'environnement ESP-IDF (export.bat / export.sh) ; vide si introuvable.</summary>
        string DetectExportScript();
    }
}
