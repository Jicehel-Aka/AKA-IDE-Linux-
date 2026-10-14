using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;

namespace GamebuinoAKA.App.ViewModels
{
    /// <summary>
    /// Moniteur série : affiche sur le PC la sortie console de l'AKA réelle
    /// (REPL MicroPython, print() d'un jeu, logs natifs) reçue par le port USB-série,
    /// et permet d'y renvoyer des caractères. Pendant du simulateur PC (écran Scripts),
    /// qui montre, lui, la console d'un programme exécuté sur le PC.
    /// </summary>
    public sealed partial class ConsoleViewModel : ViewModelBase, IDisposable
    {
        private readonly ISerialMonitorService _serial;
        private readonly ISettingsService _settings;
        private readonly INavigationService _nav;
        private string _lineBuf = "";

        public ObservableCollection<string> Ports { get; } = new();
        public int[] BaudRates { get; } = { 9600, 74880, 115200, 230400, 460800, 921600 };

        /// <summary>Emplacements d'erreur détectés dans le flux série (cliquables).</summary>
        public ObservableCollection<ErrorLocation> Errors { get; } = new();
        public bool HasErrors => Errors.Count > 0;

        [ObservableProperty] private string? _selectedPort;
        [ObservableProperty] private int _selectedBaud = 115200;
        [ObservableProperty] private bool _isConnected;
        [ObservableProperty] private string _log = "";
        [ObservableProperty] private string _input = "";
        [ObservableProperty] private bool _autoScroll = true;
        [ObservableProperty] private string _status = "Sélectionnez le port de l'AKA puis Connecter.";

        public ConsoleViewModel(ISerialMonitorService serial, ISettingsService settings, INavigationService nav)
        {
            _serial = serial; _settings = settings; _nav = nav;
            Errors.CollectionChanged += (_, __) => OnPropertyChanged(nameof(HasErrors));
            _serial.DataReceived += OnData;
            _serial.StatusChanged += OnStatus;
            RefreshPorts();
            // Port par défaut : celui des Paramètres (partagé avec « Envoyer vers l'AKA »).
            var pref = _settings.Settings.IdfSerialPort;
            if (!string.IsNullOrWhiteSpace(pref))
                SelectedPort = Ports.Contains(pref) ? pref : (Ports.FirstOrDefault() ?? pref);
            else
                SelectedPort = Ports.FirstOrDefault();
        }

        [RelayCommand]
        private void RefreshPorts()
        {
            var keep = SelectedPort;
            Ports.Clear();
            foreach (var p in _serial.ListPorts()) Ports.Add(p);
            SelectedPort = keep != null && Ports.Contains(keep) ? keep : Ports.FirstOrDefault();
            Status = Ports.Count == 0
                ? "Aucun port série détecté. Branchez l'AKA puis Actualiser."
                : $"{Ports.Count} port(s) détecté(s).";
        }

        [RelayCommand]
        private void Connect()
        {
            if (IsConnected) return;
            if (string.IsNullOrWhiteSpace(SelectedPort)) { Status = "Aucun port sélectionné."; return; }
            try
            {
                _serial.Open(SelectedPort!, SelectedBaud);
                IsConnected = true;
                // Mémorise le port pour « Envoyer vers l'AKA » (écran Scripts) et la prochaine session.
                _settings.Settings.IdfSerialPort = SelectedPort!;
                _settings.Save();
            }
            catch (Exception ex) { Append("[Erreur] " + ex.Message + "\n"); Status = "Échec de connexion."; }
        }

        [RelayCommand]
        private void Disconnect()
        {
            if (!IsConnected) return;
            _serial.Close();
            IsConnected = false;
        }

        [RelayCommand]
        private void Send()
        {
            if (!IsConnected) { Status = "Connectez-vous d'abord."; return; }
            try
            {
                _serial.Send(Input + "\r\n");   // CRLF : compatible REPL MicroPython
                Append("> " + Input + "\n");
                Input = "";
            }
            catch (Exception ex) { Append("[Erreur] " + ex.Message + "\n"); }
        }

        /// <summary>Ctrl-C (0x03) : interrompt le programme en cours sur la carte (REPL).</summary>
        [RelayCommand]
        private void SendCtrlC()
        {
            if (!IsConnected) return;
            try { _serial.Send("\x03"); Append("[Ctrl-C]\n"); } catch (Exception ex) { Append("[Erreur] " + ex.Message + "\n"); }
        }

        [RelayCommand]
        private void Clear() { Log = ""; _lineBuf = ""; Errors.Clear(); }

        [RelayCommand]
        private void GoToError(ErrorLocation? loc)
        {
            if (loc != null) _nav.OpenScriptAt(loc.File, loc.Line);
        }

        private void OnData(string text) => Append(text);
        private void OnStatus(string line) => Dispatcher.UIThread.Post(() => { Status = line; Log += line + "\n"; });

        private void Append(string text) => Dispatcher.UIThread.Post(() =>
        {
            Log += text;
            // Analyse ligne par ligne (le flux série arrive par fragments arbitraires).
            _lineBuf += text;
            int nl;
            while ((nl = _lineBuf.IndexOf('\n')) >= 0)
            {
                var lineStr = _lineBuf.Substring(0, nl);
                _lineBuf = _lineBuf.Substring(nl + 1);
                var loc = TracebackParser.ParseLine(lineStr);
                if (loc != null) Errors.Add(loc);
            }
        });

        public void Dispose()
        {
            _serial.DataReceived -= OnData;
            _serial.StatusChanged -= OnStatus;
            _serial.Close();
        }
    }
}
