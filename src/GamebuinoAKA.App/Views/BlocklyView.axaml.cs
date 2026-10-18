// Copyright (c) Gamebuino AKA. Tous droits réservés.
// Licence : GPL-3.0-or-later

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GamebuinoAKA.App.ViewModels;

namespace GamebuinoAKA.App.Views
{
    public partial class BlocklyView : UserControl
    {
        public BlocklyView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Pont Studio -> IDE : récupère le code copié dans le Studio Blockly (presse-papiers)
        // et le place dans l'aperçu, prêt à être envoyé vers l'éditeur de scripts.
        private async void OnPasteFromStudio(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not BlocklyViewModel vm) return;
            try
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard is null)
                {
                    vm.StatusMessage = "Presse-papiers indisponible sur cette plateforme.";
                    return;
                }
                var text = await clipboard.GetTextAsync();
                vm.SetPastedCode(text);
            }
            catch (System.Exception ex)
            {
                vm.StatusMessage = "Erreur de lecture du presse-papiers : " + ex.Message;
            }
        }
    }
}
