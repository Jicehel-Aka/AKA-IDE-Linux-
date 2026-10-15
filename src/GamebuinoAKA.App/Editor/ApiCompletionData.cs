using System;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.App.Editor
{
    /// <summary>
    /// Entrée d'autocomplétion pour un symbole de l'API AKA (module <c>aka</c> ou Love2D).
    /// Le texte inséré est le nom pleinement qualifié (ex. <c>aka.fill_rect</c>) ;
    /// la description affiche la signature puis la doc.
    /// </summary>
    public sealed class ApiCompletionData : ICompletionData
    {
        private readonly ApiSymbol _symbol;

        public ApiCompletionData(ApiSymbol symbol) => _symbol = symbol;

        public IImage? Image => null;

        public string Text => _symbol.Name;

        // Affiché dans la liste.
        public object Content => _symbol.Name;

        // Panneau d'aide à côté de la liste (signature + description).
        public object Description =>
            string.IsNullOrEmpty(_symbol.Doc)
                ? _symbol.Signature
                : _symbol.Signature + "\n\n" + _symbol.Doc;

        // Les constantes (UP, A…) remontent un peu ; le reste à égalité.
        public double Priority => _symbol.Kind == "constant" ? 0.5 : 1.0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, _symbol.Name);
        }
    }
}
