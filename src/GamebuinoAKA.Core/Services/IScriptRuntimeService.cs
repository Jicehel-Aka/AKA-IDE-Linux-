using System;
using System.Threading;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Pilote les projets « script » (MicroPython, Love2D) : détection du type,
    /// exécution sur le simulateur PC embarqué (qui reproduit les limites de l'AKA),
    /// et envoi du script vers la console réelle via akatransfer (série).
    /// </summary>
    public interface IScriptRuntimeService
    {
        /// <summary>Déduit la nature du projet à partir de son dossier.</summary>
        ProjectKind DetectKind(string folderPath);

        /// <summary>Vrai pour un projet script (MicroPython ou Love2D).</summary>
        bool IsScript(ProjectKind kind);

        /// <summary>Chemin du simulateur PC pour ce type (override Paramètres, sinon embarqué). Vide si non script.</summary>
        string DetectSimPath(ProjectKind kind);

        /// <summary>Script d'entrée du projet (main.py / main.lua) ou null si absent.</summary>
        string? FindEntryScript(string folderPath, ProjectKind kind);

        /// <summary>
        /// Lance le simulateur PC embarqué sur le projet (Love2D : le dossier du jeu ;
        /// MicroPython : le script d'entrée). Diffuse la sortie.
        /// </summary>
        Task RunOnPcAsync(string folderPath, ProjectKind kind, Action<string>? onOutput, CancellationToken ct = default);

        /// <summary>
        /// Envoie un script vers l'AKA réelle via akatransfer (série) :
        /// python akatransfer.py --port &lt;port&gt; --lang &lt;py|lua&gt; --dest &lt;dest&gt; &lt;fichier&gt;.
        /// </summary>
        Task SendToDeviceAsync(string scriptFile, ProjectKind kind, string? serialPort,
            Action<string>? onOutput, CancellationToken ct = default);
    }
}
