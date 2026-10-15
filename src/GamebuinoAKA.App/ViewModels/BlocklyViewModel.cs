// Copyright (c) Gamebuino AKA. Tous droits réservés.
// Licence : GPL-3.0-or-later

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GamebuinoAKA.App.ViewModels
{
    public sealed partial class BlocklyViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        [ObservableProperty]
        private string _statusMessage = "Prêt - Studio Blockly Gamebuino AKA";

        [ObservableProperty]
        private string _generatedPythonCode = @"# Code MicroPython généré par le Studio Blockly Gamebuino AKA
import gamebuino as gb

# Initialisation
gb.display.clear()
gb.display.print(16, 16, ""Hello Gamebuino AKA!"")
gb.display.update()

# Boucle principale 60 FPS
while True:
    gb.display.clear()
    if gb.buttons.pressed(gb.buttons.KEY_A):
        gb.sound.tone(880, 100)
    gb.display.update()
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
                var url = "https://ais-dev-i65yv6scujyfee7ezom4in-393028584716.europe-west2.run.app";
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
                _main.NavigateToScriptEditor();
                StatusMessage = "Basculé vers l'éditeur de scripts.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
        }
    }
}
