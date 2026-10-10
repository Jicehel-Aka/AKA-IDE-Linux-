using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Platform;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Détection de type + exécution PC (simulateur embarqué) + envoi série (akatransfer).
    ///
    /// Les simulateurs sont EMBARQUÉS avec l'IDE sous &lt;base&gt;/sims/ :
    ///   aka-love-sim[.exe]  — reproduit l'AKA (320×240 BGR565, 4 voies audio, boutons…)
    ///   aka-py-sim[.exe]    — MicroPython + module aka, mêmes limites
    ///   akatransfer.py      — transfert série vers la console
    /// Un chemin configuré dans les Paramètres a toujours la priorité sur l'embarqué.
    ///
    /// Contrat de ligne de commande attendu des simulateurs :
    ///   aka-love-sim  "&lt;dossier_du_jeu&gt;"      (dossier contenant main.lua/conf.lua)
    ///   aka-py-sim    "&lt;script.py&gt;"            (script d'entrée)
    /// </summary>
    public sealed class ScriptRuntimeService : IScriptRuntimeService
    {
        private readonly ISettingsService _settings;
        private readonly IProcessRunner _runner;
        private readonly IToolLocator _tools;

        public ScriptRuntimeService(ISettingsService settings, IProcessRunner runner, IToolLocator tools)
        {
            _settings = settings;
            _runner = runner;
            _tools = tools;
        }

        // ── Détection ────────────────────────────────────────────────────────────
        public ProjectKind DetectKind(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return ProjectKind.PlatformIO;

            // Love2D : main.lua / conf.lua (racine ou sous-dossier de jeu usuel).
            if (HasFile(folderPath, "main.lua") || HasFile(folderPath, "conf.lua"))
                return ProjectKind.Love2D;

            // MicroPython : main.py (ou un .py à la racine).
            if (HasFile(folderPath, "main.py") || AnyFile(folderPath, "*.py"))
                return ProjectKind.MicroPython;

            // Natif : marqueur .aka-build, platformio.ini, ou CMake ESP-IDF.
            return GamebuinoProject.DetectBuildSystem(folderPath) == BuildSystem.EspIdf
                ? ProjectKind.EspIdfNative
                : ProjectKind.PlatformIO;
        }

        public bool IsScript(ProjectKind kind)
            => kind == ProjectKind.MicroPython || kind == ProjectKind.Love2D;

        public string? FindEntryScript(string folderPath, ProjectKind kind)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath)) return null;
            if (kind == ProjectKind.Love2D)
            {
                var m = Path.Combine(folderPath, "main.lua");
                return File.Exists(m) ? m : FirstFile(folderPath, "*.lua");
            }
            if (kind == ProjectKind.MicroPython)
            {
                var m = Path.Combine(folderPath, "main.py");
                return File.Exists(m) ? m : FirstFile(folderPath, "*.py");
            }
            return null;
        }

        // ── Simulateur PC ──────────────────────────────────────────────────────────
        public string DetectSimPath(ProjectKind kind)
        {
            string? configured = kind switch
            {
                ProjectKind.Love2D      => _settings.Settings.LoveSimPath,
                ProjectKind.MicroPython => _settings.Settings.MicroPythonSimPath,
                _ => null
            };
            if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured)) return configured!;

            var name = kind == ProjectKind.Love2D ? "aka-love-sim" : "aka-py-sim";
            if (OperatingSystem.IsWindows()) name += ".exe";
            var embedded = Path.Combine(AppContext.BaseDirectory, "sims", name);
            return File.Exists(embedded) ? embedded : string.Empty;
        }

        public async Task RunOnPcAsync(string folderPath, ProjectKind kind,
            Action<string>? onOutput, CancellationToken ct = default)
        {
            if (!IsScript(kind))
                throw new InvalidOperationException("Projet non-script : utilisez Build/Flash ou l'émulateur.");

            var sim = DetectSimPath(kind);
            if (string.IsNullOrEmpty(sim))
                throw new InvalidOperationException(
                    $"Simulateur {(kind == ProjectKind.Love2D ? "Love2D" : "MicroPython")} introuvable. " +
                    "Renseignez son chemin dans les Paramètres (ou installez la version embarquée).");

            // Love : on passe le dossier du jeu ; MicroPython : le script d'entrée.
            string target;
            if (kind == ProjectKind.Love2D)
                target = Path.GetFullPath(folderPath);
            else
                target = FindEntryScript(folderPath, kind)
                    ?? throw new InvalidOperationException("Script d'entrée introuvable (main.py).");

            var req = new ProcessRequest(sim, new[] { target }, Path.GetDirectoryName(Path.GetFullPath(sim)));
            onOutput?.Invoke($"[Sim PC] {Path.GetFileName(sim)} {target}");
            var code = await _runner.RunStreamingAsync(req, onOutput, ct).ConfigureAwait(false);
            onOutput?.Invoke(code == 0 ? "[Sim PC] Fermé (exit 0)." : $"[Sim PC] Terminé (exit {code}).");
        }

        // ── Envoi vers la console (akatransfer) ────────────────────────────────────
        public async Task SendToDeviceAsync(string scriptFile, ProjectKind kind, string? serialPort,
            Action<string>? onOutput, CancellationToken ct = default)
        {
            if (!File.Exists(scriptFile))
                throw new InvalidOperationException($"Fichier introuvable : {scriptFile}");
            if (string.IsNullOrWhiteSpace(serialPort))
                throw new InvalidOperationException("Port série non défini (Paramètres).");

            var transfer = ResolveAkaTransfer();
            if (string.IsNullOrEmpty(transfer))
                throw new InvalidOperationException("akatransfer.py introuvable (Paramètres ou version embarquée).");

            var python = _tools.Locate("python3") ?? _tools.Locate("python") ?? "python3";

            var lang = kind == ProjectKind.Love2D ? "lua" : "py";
            var name = Path.GetFileName(scriptFile);
            // Destination sur la carte, relative à la racine du langage côté firmware.
            var dest = kind == ProjectKind.Love2D ? $"games/{name}" : $"py/{name}";

            var args = new List<string>
            {
                transfer, "--port", serialPort!, "--lang", lang, "--dest", dest, Path.GetFullPath(scriptFile)
            };
            onOutput?.Invoke($"[Envoi AKA] {Path.GetFileName(python)} {string.Join(' ', args)}");
            var code = await _runner.RunStreamingAsync(new ProcessRequest(python, args), onOutput, ct)
                .ConfigureAwait(false);
            onOutput?.Invoke(code == 0 ? "[Envoi AKA] Terminé (exit 0)." : $"[Envoi AKA] Échec (exit {code}).");
        }

        private string ResolveAkaTransfer()
        {
            var configured = _settings.Settings.AkaTransferPath;
            if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured)) return configured!;
            var embedded = Path.Combine(AppContext.BaseDirectory, "sims", "akatransfer.py");
            return File.Exists(embedded) ? embedded : string.Empty;
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private static bool HasFile(string dir, string file) => File.Exists(Path.Combine(dir, file));

        private static bool AnyFile(string dir, string pattern)
        {
            try { return Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly).Any(); }
            catch { return false; }
        }

        private static string? FirstFile(string dir, string pattern)
        {
            try { return Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly)
                        .OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault(); }
            catch { return null; }
        }
    }
}
