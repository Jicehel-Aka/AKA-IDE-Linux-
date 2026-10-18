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
        private string _generatedPythonCode = @"# Modèle MicroPython généré par le Studio Blockly Gamebuino AKA.
# API réelle de la console : module natif `aka`.
import aka

x = 160
while aka.update():          # fin de frame : écran + entrées + cadence ~60 FPS
    aka.clear()
    btn = aka.buttons()      # masque des boutons enfoncés
    if btn & aka.LEFT:
        x -= 2
    if btn & aka.RIGHT:
        x += 2
    aka.set_color(0, 200, 255)
    aka.fill_rect(x - 8, 112, 16, 16)
    aka.set_color(255, 255, 255)
    aka.text(8, 8, ""Blockly -> MicroPython AKA"")
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
