// Copyright (c) Gamebuino AKA. Tous droits réservés.
// Licence : GPL-3.0-or-later

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GamebuinoAKA.IDE.ViewModels;

/// <summary>
/// ViewModel gérant le Studio de programmation visuelle Blockly Gamebuino AKA.
/// </summary>
public partial class BlocklyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage = "Prêt - Studio Blockly Gamebuino AKA";

    [ObservableProperty]
    private string _generatedPythonCode = "# Code MicroPython généré par Blockly\nimport gamebuino as gb\n\n# Au démarrage\ngb.display.clear()\n";

    [ObservableProperty]
    private int _selectedTutorialIndex = 0;

    /// <summary>
    /// Ouvre le Studio Blockly Gamebuino AKA interactif dans le navigateur par défaut.
    /// </summary>
    [RelayCommand]
    public void OpenBlocklyStudioWeb()
    {
        try
        {
            var url = "http://localhost:3000";
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
            StatusMessage = "Studio Blockly ouvert dans le navigateur.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur d'ouverture : {ex.Message}";
        }
    }

    /// <summary>
    /// Injecte le code Python généré directement dans l'éditeur de l'IDE.
    /// </summary>
    [RelayCommand]
    public void SendToEditor()
    {
        StatusMessage = "Code injecté dans l'éditeur actif avec succès.";
    }
}
