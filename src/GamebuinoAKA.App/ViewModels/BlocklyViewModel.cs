// Copyright (c) Gamebuino AKA. Tous droits réservés.
// Licence : GPL-3.0-or-later

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.App.ViewModels
{
    public sealed partial class BlocklyViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        [ObservableProperty]
        private string _statusMessage = "Prêt - Studio Blockly Gamebuino AKA";

        [ObservableProperty]
        private string _generatedPythonCode = @"# 1) Clique « Ouvrir le Studio Blockly » (le studio s'ouvre dans ton navigateur).
# 2) Construis ton programme en blocs — ou suis le tutoriel guidé « Pong ».
# 3) Dans le studio, coche « MicroPython » et clique « Copier le code ».
# 4) Reviens ici, clique « Coller le code du Studio » : ton code apparaît ci-dessous.
# 5) Clique « Envoyer vers l'éditeur » pour l'ouvrir dans l'éditeur de scripts
#    (puis « Tester (PC) » au simulateur ou envoi sur la carte).
#
# API réelle de la console : module natif `aka`.
import aka

while aka.update():          # fin de frame : écran + entrées + cadence ~60 FPS
    aka.clear()
    aka.set_color(255, 255, 255)
    aka.text(8, 8, ""Colle ici le code de ton Studio Blockly"")
";

        public BlocklyViewModel(MainViewModel main)
        {
            _main = main;
        }

        [RelayCommand]
        public void OpenBlocklyStudioWeb()
        {
            try
            {
                var url = "https://jicehel-aka.github.io/AKA-IDE-Linux-/";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                StatusMessage = "Studio Blockly ouvert dans votre navigateur.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur d'ouverture : {ex.Message}";
            }
        }

        /// <summary>Reçoit le code copié depuis le Studio (presse-papiers) et le place dans l'aperçu.</summary>
        public void SetPastedCode(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                StatusMessage = "Presse-papiers vide : copie d'abord le code dans le Studio (bouton « Copier le code »).";
                return;
            }
            GeneratedPythonCode = text.Replace("\r\n", "\n");
            StatusMessage = "Code collé depuis le Studio. Clique « Envoyer vers l'éditeur ».";
        }

        [RelayCommand]
        public void SendToScriptEditor()
        {
            try
            {
                // Injecte réellement le code généré dans l'éditeur (non enregistré),
                // prêt à « Tester (PC) » ou à envoyer sur la carte.
                _main.OpenScriptContent("blockly.py", GeneratedPythonCode, ProjectKind.MicroPython);
                StatusMessage = "Code envoyé vers l'éditeur de scripts.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
        }
    }
}
