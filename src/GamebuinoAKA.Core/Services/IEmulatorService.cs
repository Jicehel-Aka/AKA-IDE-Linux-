using System;
using System.Threading;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    public interface IEmulatorService
    {
        /// <summary>Chemin résolu de l'émulateur (run.sh / aka-front[.exe]) ou vide.</summary>
        string DetectEmulatorPath();

        bool IsAvailable();

        /// <summary>
        /// Firmware du projet à lancer : build/&lt;nom&gt;.bin (ESP-IDF) ou .pio/.../firmware.bin
        /// (PlatformIO). Renvoie null si introuvable (projet pas encore compilé).
        /// </summary>
        string? ResolveFirmware(GamebuinoProject project);

        /// <summary>Dossier d'assets SD du projet à exposer à l'émulateur, ou null.</summary>
        string? DetectSdFolder(GamebuinoProject project);

        /// <summary>
        /// Lance l'émulateur en BOOT DIRECT sur le firmware du projet
        /// (aka-front --game &lt;fw&gt; --boot-game [--sd &lt;dossier&gt;]). Diffuse la sortie.
        /// </summary>
        Task LaunchAsync(GamebuinoProject project, Action<string>? onOutput, CancellationToken ct = default);

        /// <summary>
        /// Lance l'émulateur en boot direct sur un firmware donné, avec un dossier SD
        /// optionnel (sert aux interpréteurs : MicroPython, AKA-Love).
        /// </summary>
        Task LaunchFirmwareAsync(string firmwarePath, string? sdFolder,
            Action<string>? onOutput, CancellationToken ct = default);
    }
}
