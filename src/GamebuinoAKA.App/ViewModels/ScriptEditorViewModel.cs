using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;

namespace GamebuinoAKA.App.ViewModels
{
    /// <summary>
    /// Éditeur de scripts MicroPython (.py) et Love2D (.lua) : ouvrir/enregistrer,
    /// tester sur le simulateur PC embarqué, envoyer vers l'AKA réelle (série).
    /// </summary>
    public sealed partial class ScriptEditorViewModel : ViewModelBase
    {
        private readonly IScriptRuntimeService _runtime;
        private readonly ISettingsService _settings;
        private readonly IFileDialogService _files;
        private readonly IApiCatalogService _api;

        [ObservableProperty] private string _scriptText = "";
        [ObservableProperty] private string _currentFile = "";
        [ObservableProperty] private ProjectKind _kind = ProjectKind.MicroPython;
        [ObservableProperty] private string _outputLog = "";
        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private string _status = "Ouvrez ou créez un script .py (MicroPython) ou .lua (Love2D).";

        public string KindLabel => Kind switch
        {
            ProjectKind.Love2D => "Love2D (.lua)",
            ProjectKind.MicroPython => "MicroPython (.py)",
            _ => "—"
        };
        public bool HasFile => !string.IsNullOrEmpty(CurrentFile);
        /// <summary>Symboles d'API du langage courant (autocomplétion + aide au survol).</summary>
        public IReadOnlyList<ApiSymbol> ApiSymbols => _api.GetSymbols(Kind);

        /// <summary>Emplacements d'erreur détectés dans la dernière exécution (cliquables).</summary>
        public ObservableCollection<ErrorLocation> Errors { get; } = new();
        public bool HasErrors => Errors.Count > 0;

        /// <summary>Ligne vers laquelle l'éditeur doit sauter (la vue s'y abonne).</summary>
        [ObservableProperty] private int? _gotoLine;
        /// <summary>Syntaxe AvaloniaEdit à appliquer ("Python" / "Lua" / null).</summary>
        public string? Highlighting => Kind == ProjectKind.Love2D ? "Lua"
            : Kind == ProjectKind.MicroPython ? "Python" : null;

        partial void OnKindChanged(ProjectKind value)
        {
            OnPropertyChanged(nameof(KindLabel));
            OnPropertyChanged(nameof(Highlighting));
            OnPropertyChanged(nameof(ApiSymbols));
        }
        partial void OnCurrentFileChanged(string value) => OnPropertyChanged(nameof(HasFile));

        public ScriptEditorViewModel(IScriptRuntimeService runtime, ISettingsService settings,
            IFileDialogService files, IApiCatalogService api)
        {
            _runtime = runtime; _settings = settings; _files = files; _api = api;
            Errors.CollectionChanged += (_, __) => OnPropertyChanged(nameof(HasErrors));
        }

        [RelayCommand]
        private async Task Open()
        {
            var path = await _files.OpenFileAsync("Ouvrir un script",
                new[] { new FileFilter("Scripts AKA", new[] { "py", "lua" }) });
            if (path is null) return;
            try
            {
                ScriptText = await File.ReadAllTextAsync(path);
                CurrentFile = path;
                Kind = Path.GetExtension(path).Equals(".lua", StringComparison.OrdinalIgnoreCase)
                    ? ProjectKind.Love2D : ProjectKind.MicroPython;
                Status = $"Ouvert : {path}";
            }
            catch (Exception ex) { Status = "Erreur : " + ex.Message; }
        }

        [RelayCommand]
        private async Task Save()
        {
            var path = CurrentFile;
            if (string.IsNullOrEmpty(path))
            {
                var suggested = Kind == ProjectKind.Love2D ? "main.lua" : "main.py";
                path = await _files.SaveFileAsync("Enregistrer le script", suggested,
                    new[] { new FileFilter("Scripts AKA", new[] { "py", "lua" }) });
                if (path is null) return;
            }
            try
            {
                await File.WriteAllTextAsync(path, ScriptText);
                CurrentFile = path;
                Status = $"Enregistré : {path}";
            }
            catch (Exception ex) { Status = "Erreur : " + ex.Message; }
        }

        [RelayCommand]
        private async Task RunOnPc()
        {
            if (!await EnsureSaved()) return;
            var folder = Path.GetDirectoryName(Path.GetFullPath(CurrentFile))!;
            var kind = _runtime.DetectKind(folder);
            if (!_runtime.IsScript(kind)) kind = Kind; // repli sur l'extension
            IsBusy = true; OutputLog = ""; Errors.Clear();
            try { await _runtime.RunOnPcAsync(folder, kind, Append); }
            catch (Exception ex) { Append("Erreur : " + ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SendToDevice()
        {
            if (!await EnsureSaved()) return;
            IsBusy = true;
            try { await _runtime.SendToDeviceAsync(CurrentFile, Kind, _settings.Settings.IdfSerialPort, Append); }
            catch (Exception ex) { Append("Erreur : " + ex.Message); }
            finally { IsBusy = false; }
        }

        private async Task<bool> EnsureSaved()
        {
            if (string.IsNullOrEmpty(CurrentFile)) { await Save(); }
            else { try { await File.WriteAllTextAsync(CurrentFile, ScriptText); } catch (Exception ex) { Status = "Erreur : " + ex.Message; return false; } }
            return !string.IsNullOrEmpty(CurrentFile);
        }

        private void Append(string line) => Dispatcher.UIThread.Post(() =>
        {
            OutputLog += line + "\n";
            foreach (var sub in line.Split('\n'))
            {
                var loc = TracebackParser.ParseLine(sub);
                if (loc != null) Errors.Add(loc);
            }
        });

        [RelayCommand]
        private void GoToError(ErrorLocation? loc)
        {
            if (loc != null) OpenFileAt(loc.File, loc.Line);
        }

        /// <summary>
        /// Ouvre le fichier (résolu relativement au script courant si nécessaire) et
        /// positionne l'éditeur à la ligne donnée. « &lt;stdin&gt; » ou fichier introuvable :
        /// on reste sur le fichier courant et on saute quand même à la ligne.
        /// </summary>
        public void OpenFileAt(string file, int line)
        {
            try
            {
                string? resolved = null;
                if (!string.IsNullOrWhiteSpace(file) && file != "<stdin>")
                {
                    if (File.Exists(file)) resolved = file;
                    else if (!string.IsNullOrEmpty(CurrentFile))
                    {
                        var folder = Path.GetDirectoryName(Path.GetFullPath(CurrentFile));
                        if (folder != null)
                        {
                            var cand = Path.Combine(folder, Path.GetFileName(file));
                            if (File.Exists(cand)) resolved = cand;
                        }
                    }
                }

                if (resolved != null &&
                    !string.Equals(Path.GetFullPath(resolved),
                        string.IsNullOrEmpty(CurrentFile) ? "" : Path.GetFullPath(CurrentFile),
                        StringComparison.OrdinalIgnoreCase))
                {
                    ScriptText = File.ReadAllText(resolved);
                    CurrentFile = resolved;
                    Kind = Path.GetExtension(resolved).Equals(".lua", StringComparison.OrdinalIgnoreCase)
                        ? ProjectKind.Love2D : ProjectKind.MicroPython;
                }

                // Toggle pour forcer le déclenchement même si on reste sur la même ligne.
                GotoLine = null;
                GotoLine = line;
            }
            catch (Exception ex) { Status = "Erreur : " + ex.Message; }
        }
    }
}
