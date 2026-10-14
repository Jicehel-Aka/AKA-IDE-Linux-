using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Highlighting;
using GamebuinoAKA.App.Editor;
using GamebuinoAKA.App.ViewModels;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.App.Views
{
    public partial class ScriptEditorView : UserControl
    {
        private TextEditor? _editor;
        private ScriptEditorViewModel? _vm;
        private bool _sync;

        private CompletionWindow? _completion;
        private readonly Dictionary<string, ApiSymbol> _lookup = new();
        private string? _lastHoverWord;

        public ScriptEditorView()
        {
            AvaloniaXamlLoader.Load(this);
            _editor = this.FindControl<TextEditor>("Editor");
            if (_editor != null)
            {
                _editor.TextChanged += (_, _) =>
                {
                    if (_sync || _vm is null) return;
                    _sync = true; _vm.ScriptText = _editor.Text; _sync = false;
                };
                _editor.TextArea.TextEntered += OnTextEntered;
                _editor.AddHandler(KeyDownEvent, OnEditorKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                _editor.PointerMoved += OnPointerMoved;
            }
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            if (_vm != null) _vm.PropertyChanged -= OnVmPropertyChanged;
            _vm = DataContext as ScriptEditorViewModel;
            if (_vm != null)
            {
                _vm.PropertyChanged += OnVmPropertyChanged;
                SetEditorText(_vm.ScriptText);
                ApplyHighlighting(_vm.Highlighting);
                RebuildLookup();
            }
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_vm is null) return;
            if (e.PropertyName == nameof(ScriptEditorViewModel.ScriptText)) SetEditorText(_vm.ScriptText);
            else if (e.PropertyName == nameof(ScriptEditorViewModel.Highlighting)) ApplyHighlighting(_vm.Highlighting);
            else if (e.PropertyName == nameof(ScriptEditorViewModel.ApiSymbols)) RebuildLookup();
        }

        // ── Autocomplétion ───────────────────────────────────────────────────────
        private void RebuildLookup()
        {
            _lookup.Clear();
            if (_vm?.ApiSymbols is null) return;
            foreach (var s in _vm.ApiSymbols) _lookup[s.Name] = s;
        }

        private void OnEditorKeyDown(object? sender, KeyEventArgs e)
        {
            // Ctrl+Espace : complétion explicite.
            if (e.Key == Key.Space && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                ShowCompletion();
                e.Handled = true;
            }
        }

        private void OnTextEntered(object? sender, TextInputEventArgs e)
        {
            if (_editor is null || _vm is null || _lookup.Count == 0) return;
            if (string.IsNullOrEmpty(e.Text)) return;
            char c = e.Text[0];
            // Ouvre sur « . » (ex. après « aka ») ou en tapant un identifiant, si rien d'ouvert.
            if (c == '.' || char.IsLetter(c))
            {
                if (_completion is null) ShowCompletion();
            }
        }

        private void ShowCompletion()
        {
            if (_editor is null || _vm?.ApiSymbols is null || _vm.ApiSymbols.Count == 0) return;

            // Segment à remplacer : le mot « pointé » courant (lettres, chiffres, _ et .).
            var doc = _editor.Document;
            int caret = _editor.CaretOffset;
            int start = caret;
            while (start > 0)
            {
                char ch = doc.GetCharAt(start - 1);
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '.') start--;
                else break;
            }

            _completion = new CompletionWindow(_editor.TextArea);
            _completion.StartOffset = start;
            _completion.EndOffset = caret;
            var data = _completion.CompletionList.CompletionData;
            foreach (var s in _vm.ApiSymbols) data.Add(new ApiCompletionData(s));
            _completion.Closed += (_, __) => _completion = null;
            _completion.Show();
        }

        // ── Aide au survol ───────────────────────────────────────────────────────
        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_editor is null || _lookup.Count == 0) return;
            try
            {
                var pos = _editor.GetPositionFromPoint(e.GetPosition(_editor));
                if (pos is null) { HideHover(); return; }
                var loc = pos.Value;
                var line = _editor.Document.GetLineByNumber(loc.Line);
                var text = _editor.Document.GetText(line);
                var word = WordAt(text, loc.Column - 1);
                if (word is null || !_lookup.TryGetValue(word, out var sym)) { HideHover(); return; }
                if (word == _lastHoverWord) return;
                _lastHoverWord = word;
                var tip = string.IsNullOrEmpty(sym.Doc) ? sym.Signature : sym.Signature + "\n\n" + sym.Doc;
                ToolTip.SetTip(_editor, tip);
                ToolTip.SetIsOpen(_editor, true);
            }
            catch { HideHover(); }
        }

        private void HideHover()
        {
            if (_editor is null || _lastHoverWord is null) return;
            _lastHoverWord = null;
            ToolTip.SetIsOpen(_editor, false);
        }

        private static string? WordAt(string text, int index)
        {
            if (index < 0 || index > text.Length) return null;
            static bool Ok(char ch) => char.IsLetterOrDigit(ch) || ch == '_' || ch == '.';
            int start = index, end = index;
            while (start > 0 && Ok(text[start - 1])) start--;
            while (end < text.Length && Ok(text[end])) end++;
            if (end <= start) return null;
            var w = text.Substring(start, end - start).Trim('.');
            return w.Length == 0 ? null : w;
        }

        // ── Éditeur ↔ VM ───────────────────────────────────────────────────────
        private void SetEditorText(string text)
        {
            if (_editor is null || _sync) return;
            if (_editor.Text == text) return;
            _sync = true; _editor.Text = text ?? ""; _sync = false;
        }

        private void ApplyHighlighting(string? name)
        {
            if (_editor is null) return;
            _editor.SyntaxHighlighting = string.IsNullOrEmpty(name)
                ? null
                : HighlightingManager.Instance.GetDefinition(name);
        }
    }
}
