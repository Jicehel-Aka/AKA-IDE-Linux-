using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Platform;
using Newtonsoft.Json.Linq;

namespace GamebuinoAKA.Core.Services
{
    public sealed class EspIdfService : IEspIdfService
    {
        private readonly ISettingsService _settings;
        private readonly IProcessRunner _runner;

        public EspIdfService(ISettingsService settings, IProcessRunner runner)
        {
            _settings = settings;
            _runner = runner;
        }

        public Task BuildAsync(GamebuinoProject p, Action<string>? o, CancellationToken ct = default)
            => RunIdf(p, o, ct, "build");

        public Task FlashAsync(GamebuinoProject p, Action<string>? o, CancellationToken ct = default)
            => RunIdf(p, o, ct, PortPrefix() + "flash");

        public Task MonitorAsync(GamebuinoProject p, Action<string>? o, CancellationToken ct = default)
            => RunIdf(p, o, ct, PortPrefix() + "monitor");

        public Task CleanAsync(GamebuinoProject p, Action<string>? o, CancellationToken ct = default)
            => RunIdf(p, o, ct, "fullclean");

        private string PortPrefix()
        {
            var port = _settings.Settings.IdfSerialPort;
            return string.IsNullOrWhiteSpace(port) ? string.Empty : $"-p {port} ";
        }

        private async Task RunIdf(GamebuinoProject p, Action<string>? onOutput, CancellationToken ct, string sub)
        {
            var request = BuildRequest(p.FolderPath, sub);
            onOutput?.Invoke($"[ESP-IDF] idf.py -C {p.FolderPath} {sub}");
            var code = await _runner.RunStreamingAsync(request, onOutput, ct).ConfigureAwait(false);
            onOutput?.Invoke(code == 0 ? "[ESP-IDF] Terminé avec succès (exit 0)." : $"[ESP-IDF] Échec (exit {code}).");
        }

        private ProcessRequest BuildRequest(string projectDir, string sub)
        {
            var export = _settings.Settings.IdfExportScript;
            if (OperatingSystem.IsWindows())
            {
                var call = !string.IsNullOrWhiteSpace(export) ? $"call \"{export}\" && " : string.Empty;
                var cmd = $"{call}idf.py -C \"{projectDir}\" {sub}";
                return new ProcessRequest("cmd.exe", new[] { "/c", cmd }, projectDir);
            }
            else
            {
                var source = !string.IsNullOrWhiteSpace(export) ? $"source \"{export}\" && " : string.Empty;
                var cmd = $"{source}idf.py -C \"{projectDir}\" {sub}";
                return new ProcessRequest("bash", new[] { "-lc", cmd }, projectDir);
            }
        }

        public async Task<string> GetVersionAsync()
        {
            try
            {
                var req = BuildRequestRaw("idf.py --version");
                var result = await _runner.RunAsync(req);
                return result.StandardOutput.Trim();
            }
            catch { return "Non détecté"; }
        }

        private ProcessRequest BuildRequestRaw(string idfCommand)
        {
            var export = _settings.Settings.IdfExportScript;
            if (OperatingSystem.IsWindows())
            {
                var call = !string.IsNullOrWhiteSpace(export) ? $"call \"{export}\" && " : string.Empty;
                return new ProcessRequest("cmd.exe", new[] { "/c", call + idfCommand });
            }
            var source = !string.IsNullOrWhiteSpace(export) ? $"source \"{export}\" && " : string.Empty;
            return new ProcessRequest("bash", new[] { "-lc", source + idfCommand });
        }

        /// <summary>Ports série candidats (Windows : COM* ; Linux : /dev/ttyUSB*, /dev/ttyACM*).</summary>
        public string[] DetectSerialPorts()
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    return System.IO.Ports.SerialPort.GetPortNames()
                        .Distinct().OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
                }
                catch { return Array.Empty<string>(); }
            }
            try
            {
                var ports = new List<string>();
                foreach (var pattern in new[] { "ttyUSB*", "ttyACM*" })
                    if (Directory.Exists("/dev"))
                        ports.AddRange(Directory.GetFiles("/dev", pattern));
                return ports.OrderBy(p => p, StringComparer.Ordinal).ToArray();
            }
            catch { return Array.Empty<string>(); }
        }

        /// <summary>
        /// Localise le script d'environnement ESP-IDF (export.bat / export.sh).
        /// Windows : esp_idf.json (extension VS Code) puis frameworks\esp-idf-*.
        /// Linux : IDF_PATH puis ~/esp/esp-idf, /opt/esp-idf.
        /// </summary>
        public string DetectExportScript()
        {
            var cfg = _settings.Settings.IdfExportScript;
            if (!string.IsNullOrWhiteSpace(cfg) && File.Exists(cfg)) return cfg;

            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var toolsPath = Environment.GetEnvironmentVariable("IDF_TOOLS_PATH");

            if (OperatingSystem.IsWindows())
            {
                foreach (var json in new[]
                {
                    toolsPath is null ? null : Path.Combine(toolsPath, "esp_idf.json"),
                    Path.Combine(user, ".espressif", "esp_idf.json"),
                    @"C:\ESP-IDF\esp_idf.json", @"C:\Espressif\esp_idf.json"
                })
                {
                    var dir = ReadIdfPathFromJson(json);
                    if (dir != null)
                    {
                        var e = Path.Combine(dir, "export.bat");
                        if (File.Exists(e)) return e;
                    }
                }
                foreach (var root in new[] { toolsPath, @"C:\ESP-IDF", @"C:\Espressif",
                                             Path.Combine(user, "esp"), Path.Combine(user, "Espressif") })
                {
                    if (string.IsNullOrEmpty(root)) continue;
                    foreach (var d in Frameworks(root))
                    {
                        var e = Path.Combine(d, "export.bat");
                        if (File.Exists(e)) return e;
                    }
                    var e2 = Path.Combine(root, "export.bat");
                    if (File.Exists(e2)) return e2;
                }
            }
            else
            {
                var env = Environment.GetEnvironmentVariable("IDF_PATH");
                if (!string.IsNullOrEmpty(env))
                {
                    var e = Path.Combine(env, "export.sh");
                    if (File.Exists(e)) return e;
                }
                foreach (var d in new[]
                {
                    Path.Combine(user, "esp", "esp-idf"), "/opt/esp-idf", "/opt/esp/esp-idf",
                })
                {
                    var e = Path.Combine(d, "export.sh");
                    if (File.Exists(e)) return e;
                }
                var espRoot = Path.Combine(user, "esp");
                foreach (var d in Frameworks(espRoot))
                {
                    var e = Path.Combine(d, "export.sh");
                    if (File.Exists(e)) return e;
                }
            }
            return string.Empty;
        }

        private static IEnumerable<string> Frameworks(string root)
        {
            foreach (var baseDir in new[] { Path.Combine(root, "frameworks"), root })
            {
                if (!Directory.Exists(baseDir)) continue;
                string[] dirs;
                try { dirs = Directory.GetDirectories(baseDir, "esp-idf*"); }
                catch { continue; }
                foreach (var d in dirs.OrderByDescending(x => x)) yield return d;
            }
        }

        private static string? ReadIdfPathFromJson(string? jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath)) return null;
            try
            {
                var root = JObject.Parse(File.ReadAllText(jsonPath));
                var selected = (string?)root["idfSelectedId"];
                if (root["idfInstalled"] is JObject installed)
                {
                    if (selected != null && (string?)installed[selected]?["path"] is string sp) return sp;
                    foreach (var prop in installed.Properties())
                        if ((string?)prop.Value["path"] is string p) return p;
                }
            }
            catch { /* json illisible */ }
            return null;
        }
    }
}
