using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Implémentation de <see cref="ISerialMonitorService"/> basée sur
    /// <see cref="SerialPort"/>. On ne s'appuie PAS sur l'événement
    /// <c>DataReceived</c> de SerialPort (peu fiable sous Linux/Mono) : on lit en
    /// continu sur <see cref="SerialPort.BaseStream"/> dans une tâche dédiée, ce qui
    /// fonctionne de façon identique sous Linux et Windows.
    /// </summary>
    public sealed class SerialMonitorService : ISerialMonitorService, IDisposable
    {
        private readonly object _lock = new();
        private SerialPort? _port;
        private CancellationTokenSource? _cts;
        private Task? _reader;

        public bool IsOpen { get { lock (_lock) return _port?.IsOpen == true; } }
        public string? CurrentPort { get; private set; }

        public event Action<string>? DataReceived;
        public event Action<string>? StatusChanged;

        public string[] ListPorts()
        {
            try
            {
                var names = SerialPort.GetPortNames();
                Array.Sort(names, StringComparer.OrdinalIgnoreCase);
                return names;
            }
            catch { return Array.Empty<string>(); }
        }

        public void Open(string port, int baudRate)
        {
            if (string.IsNullOrWhiteSpace(port))
                throw new InvalidOperationException("Aucun port série sélectionné.");

            Close();

            lock (_lock)
            {
                var sp = new SerialPort(port, baudRate)
                {
                    Encoding = Encoding.UTF8,
                    ReadTimeout = 500,
                    WriteTimeout = 1000,
                    DtrEnable = true,   // nécessaire pour que beaucoup d'ESP32 émettent
                    RtsEnable = true,
                    NewLine = "\n",
                };
                sp.Open();
                _port = sp;
                CurrentPort = port;

                _cts = new CancellationTokenSource();
                _reader = Task.Run(() => ReadLoopAsync(sp, _cts.Token));
            }

            StatusChanged?.Invoke($"[Série] Connecté à {port} @ {baudRate} bauds.");
        }

        private async Task ReadLoopAsync(SerialPort sp, CancellationToken ct)
        {
            var buffer = new byte[4096];
            var decoder = Encoding.UTF8.GetDecoder();
            var chars = new char[4096];
            try
            {
                var stream = sp.BaseStream;
                while (!ct.IsCancellationRequested)
                {
                    int n;
                    try
                    {
                        n = await stream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (TimeoutException) { continue; }
                    catch (Exception ex)
                    {
                        if (!ct.IsCancellationRequested)
                            StatusChanged?.Invoke("[Série] Lecture interrompue : " + ex.Message);
                        break;
                    }

                    if (n <= 0) { await Task.Delay(10, ct).ConfigureAwait(false); continue; }

                    int cc = decoder.GetChars(buffer, 0, n, chars, 0);
                    if (cc > 0) DataReceived?.Invoke(new string(chars, 0, cc));
                }
            }
            catch (OperationCanceledException) { /* fermeture normale */ }
        }

        public void Send(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            lock (_lock)
            {
                if (_port?.IsOpen != true)
                    throw new InvalidOperationException("Port série non connecté.");
                _port.Write(text);
            }
        }

        public void RunViaRawRepl(string code)
        {
            if (!IsOpen) throw new InvalidOperationException("Port série non connecté.");
            if (string.IsNullOrEmpty(code)) return;

            // Protocole raw-REPL MicroPython : Ctrl-C (interrompt), Ctrl-A (entre en
            // raw REPL), code + Ctrl-D (exécute), Ctrl-B (retour REPL amical). La
            // sortie revient par la boucle de lecture normale (affichée dans la console).
            StatusChanged?.Invoke("[REPL] Envoi du script sur la carte…");
            Task.Run(() =>
            {
                try
                {
                    Send("\r\x03\x03");           // réveil + double Ctrl-C
                    Thread.Sleep(120);
                    Send("\x01");                 // Ctrl-A : raw REPL
                    Thread.Sleep(120);
                    Send(code);
                    Send("\x04");                 // Ctrl-D : exécute
                    Thread.Sleep(120);
                    Send("\x02");                 // Ctrl-B : REPL amical
                }
                catch (Exception ex)
                {
                    StatusChanged?.Invoke("[REPL] Échec : " + ex.Message);
                }
            });
        }

        public void Close()
        {
            SerialPort? toClose;
            CancellationTokenSource? cts;
            Task? reader;
            lock (_lock)
            {
                toClose = _port; cts = _cts; reader = _reader;
                _port = null; _cts = null; _reader = null;
            }
            if (toClose is null) return;

            try { cts?.Cancel(); } catch { }
            try { reader?.Wait(1000); } catch { }
            try { if (toClose.IsOpen) toClose.Close(); } catch { }
            try { toClose.Dispose(); } catch { }
            try { cts?.Dispose(); } catch { }

            var was = CurrentPort;
            CurrentPort = null;
            StatusChanged?.Invoke($"[Série] Déconnecté{(was is null ? "" : " de " + was)}.");
        }

        public void Dispose() => Close();
    }
}
