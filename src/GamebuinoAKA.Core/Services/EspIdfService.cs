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

        public string[] DetectSerialPorts()
        {
            if (OperatingSystem.IsWindows()) return Array.Empty<string>();
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
    }
}
