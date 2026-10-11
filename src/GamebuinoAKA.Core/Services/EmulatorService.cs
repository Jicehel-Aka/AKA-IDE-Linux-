using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Platform;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Lance l'émulateur PC aka-emulator en BOOT DIRECT sur le firmware du projet,
    /// sans passer par le Launcher :
    ///   aka-front --game &lt;firmware.bin&gt; --boot-game [--sd &lt;dossier&gt;]
    /// (run.sh relaie "$@" à ./bin/aka-front sous Linux ; aka-front.exe sous Windows).
    ///
    /// Portable : localise l'émulateur via IToolLocator, l'exécute via IProcessRunner
    /// avec des arguments séparés et des chemins ABSOLUS (ils doivent résister au
    /// « cd » que run.sh fait vers son propre dossier).
    /// </summary>
    public sealed class EmulatorService : IEmulatorService
    {
        private readonly ISettingsService _settings;
        private readonly IProcessRunner _runner;
        private readonly IToolLocator _tools;

        // Binaires système à ignorer quand on cherche le firmware applicatif ESP-IDF.
        private static readonly string[] SystemBins =
        {
            "bootloader.bin", "partition-table.bin", "partitions.bin",
            "ota_data_initial.bin", "flash_args"
        };

        public EmulatorService(ISettingsService settings, IProcessRunner runner, IToolLocator tools)
        {
            _settings = settings;
            _runner = runner;
            _tools = tools;
        }

        public string DetectEmulatorPath()
        {
            var configured = _settings.Settings.EmulatorPath;
            if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
                return configured;

            // Nom attendu selon l'OS (run.sh relaie vers aka-front sous Linux).
            var name = OperatingSystem.IsWindows() ? "aka-front.exe" : "run.sh";
            return _tools.Locate(name, configured) ?? string.Empty;
        }

        public bool IsAvailable() => !string.IsNullOrEmpty(DetectEmulatorPath());

        public string? ResolveFirmware(GamebuinoProject project)
        {
            if (string.IsNullOrWhiteSpace(project.FolderPath) || !Directory.Exists(project.FolderPath))
                return null;

            // ESP-IDF : build/<app>.bin (en écartant bootloader / partition-table…).
            var buildDir = Path.Combine(project.FolderPath, "build");
            var fw = PickAppBin(buildDir, project.Name);
            if (fw != null) return fw;

            // PlatformIO : .pio/build/<env>/firmware.bin
            var pioBuild = Path.Combine(project.FolderPath, ".pio", "build");
            if (Directory.Exists(pioBuild))
            {
                foreach (var env in Directory.EnumerateDirectories(pioBuild))
                {
                    var f = Path.Combine(env, "firmware.bin");
                    if (File.Exists(f)) return f;
                }
            }
            return null;
        }

        /// <summary>Choisit le .bin applicatif dans un dossier build ESP-IDF.</summary>
        private static string? PickAppBin(string buildDir, string projectName)
        {
            if (!Directory.Exists(buildDir)) return null;

            // 1) <ProjectName>.bin en priorité (nom habituel du binaire d'app ESP-IDF).
            if (!string.IsNullOrWhiteSpace(projectName))
            {
                var named = Path.Combine(buildDir, projectName + ".bin");
                if (File.Exists(named)) return named;
            }

            // 2) Sinon, le plus gros .bin hors binaires système (l'app est la plus grosse).
            string[] bins;
            try { bins = Directory.GetFiles(buildDir, "*.bin"); }
            catch { return null; }

            return bins
                .Where(b => !SystemBins.Contains(Path.GetFileName(b), StringComparer.OrdinalIgnoreCase))
                .OrderByDescending(b => new FileInfo(b).Length)
                .FirstOrDefault();
        }

        public string? DetectSdFolder(GamebuinoProject project)
        {
            if (string.IsNullOrWhiteSpace(project.FolderPath)) return null;
            foreach (var candidate in new[] { "sdcard_files", "sdcard", "sd" })
            {
                var dir = Path.Combine(project.FolderPath, candidate);
                if (Directory.Exists(dir)) return dir;
            }
            return null;
        }

        public Task LaunchAsync(GamebuinoProject project, Action<string>? onOutput, CancellationToken ct = default)
        {
            var fw = ResolveFirmware(project)
                ?? throw new InvalidOperationException(
                    "Firmware introuvable. Compilez le projet (Build) avant de lancer l'émulateur.");
            return LaunchFirmwareAsync(fw, DetectSdFolder(project), onOutput, ct);
        }

        public async Task LaunchFirmwareAsync(string firmwarePath, string? sdFolder,
            Action<string>? onOutput, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(firmwarePath) || !File.Exists(firmwarePath))
                throw new InvalidOperationException($"Firmware introuvable : {firmwarePath}");

            var emu = DetectEmulatorPath();
            if (string.IsNullOrEmpty(emu))
                throw new InvalidOperationException(
                    "Émulateur introuvable. Renseignez son chemin (run.sh / aka-front.exe) dans les Paramètres.");

            // Chemins ABSOLUS : run.sh fait « cd » vers son dossier avant d'exécuter aka-front.
            var fwAbs = Path.GetFullPath(firmwarePath);
            var args = new List<string> { "--game", fwAbs, "--boot-game" };
            if (!string.IsNullOrWhiteSpace(sdFolder) && Directory.Exists(sdFolder))
            {
                args.Add("--sd");
                args.Add(Path.GetFullPath(sdFolder));
            }

            // WorkingDirectory = dossier de l'émulateur, pour que aka-emu.cfg et bin/ se résolvent.
            var workDir = Path.GetDirectoryName(Path.GetFullPath(emu));

            onOutput?.Invoke($"[Émulateur] {Path.GetFileName(emu)} {string.Join(' ', args)}");
            var request = new ProcessRequest(emu, args, workDir);
            var code = await _runner.RunStreamingAsync(request, onOutput, ct).ConfigureAwait(false);
            onOutput?.Invoke(code == 0
                ? "[Émulateur] Fermé (exit 0)."
                : $"[Émulateur] Terminé (exit {code}).");
        }
    }
}
