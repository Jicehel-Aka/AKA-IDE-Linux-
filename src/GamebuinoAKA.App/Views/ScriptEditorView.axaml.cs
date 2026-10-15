using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using GamebuinoAKA.App.ViewModels;

namespace GamebuinoAKA.App.Views
{
    public partial class ScriptEditorView : UserControl
    {
        private TextEditor? _editor;
        private ScriptEditorViewModel? _vm;
        private bool _sync;

        public ScriptEditorView()
        {
            AvaloniaXamlLoader.Load(this);
            _editor = this.FindControl<TextEditor>("Editor");
            if (_editor != null)
                _editor.TextChanged += (_, _) =>
                {
                    if (_sync || _vm is null) return;
                    _sync = true; _vm.ScriptText = _editor.Text; _sync = false;
                };
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
            }
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_vm is null) return;
            if (e.PropertyName == nameof(ScriptEditorViewModel.ScriptText)) SetEditorText(_vm.ScriptText);
            else if (e.PropertyName == nameof(ScriptEditorViewModel.Highlighting)) ApplyHighlighting(_vm.Highlighting);
        }

        private void SetEditorText(string text)
        {
            if (_editor is null || _sync) return;
            if (_editor.Text == text) return;
            _sync = true; _editor.Text = text ?? ""; _sync = false;
        }

        private void ApplyHighlighting(string? name)
        {
            if (_editor is null) return;
            // AvaloniaEdit n'embarque pas toutes les grammaires : on applique si dispo, sinon rien.
            _editor.SyntaxHighlighting = string.IsNullOrEmpty(name)
                ? null
                : HighlightingManager.Instance.GetDefinition(name);
        }
    }
}
