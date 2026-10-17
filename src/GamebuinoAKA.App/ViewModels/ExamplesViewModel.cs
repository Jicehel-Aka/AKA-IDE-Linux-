using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;

namespace GamebuinoAKA.App.ViewModels
{
    /// <summary>
    /// Galerie d'exemples : parcourir les scripts fournis (pong, calculatrice, feu
    /// d'artifice…), les prévisualiser, les ouvrir dans l'éditeur ou en faire un
    /// nouveau projet (avec le cadre MicroPython, prêt à « Tester (PC) »).
    /// </summary>
    public sealed partial class ExamplesViewModel : ViewModelBase
    {
        private readonly IExampleCatalogService _catalog;
        private readonly ITemplateService _templates;
        private readonly ISettingsService _settings;
        private readonly INavigationService _nav;
        private readonly IFileDialogService _files;
        private readonly IDialogService _dialogs;

        public IReadOnlyList<ExampleInfo> Examples { get; }

        [ObservableProperty] private ExampleInfo? _selected;
        [ObservableProperty] private string _preview = "";
        [ObservableProperty] private string _status = "Choisissez un exemple pour l'aperçu.";

        public bool HasSelection => Selected != null;

        public ExamplesViewModel(IExampleCatalogService catalog, ITemplateService templates,
            ISettingsService settings, INavigationService nav, IFileDialogService files, IDialogService dialogs)
        {
            _catalog = catalog; _templates = templates; _settings = settings;
            _nav = nav; _files = files; _dialogs = dialogs;
            Examples = _catalog.GetExamples();
            if (Examples.Count > 0) Selected = Examples[0];
        }

        partial void OnSelectedChanged(ExampleInfo? value)
        {
            OnPropertyChanged(nameof(HasSelection));
            Preview = value == null ? "" : _catalog.ReadSource(value);
            Status = value == null ? "Choisissez un exemple pour l'aperçu." : value.Title;
        }

        [RelayCommand]
        private void OpenInEditor()
        {
            if (Selected == null) return;
            var src = _catalog.ReadSource(Selected);
            if (string.IsNullOrEmpty(src)) { Status = "Exemple introuvable."; return; }
            _nav.OpenScriptContent(Selected.Id + ".py", src, ProjectKind.MicroPython);
        }

        [RelayCommand]
        private async Task CreateProject()
        {
            if (Selected == null) return;
            var src = _catalog.ReadSource(Selected);
            if (string.IsNullOrEmpty(src)) { Status = "Exemple introuvable."; return; }

            var start = string.IsNullOrWhiteSpace(_settings.Settings.WorkspaceFolder)
                ? null : _settings.Settings.WorkspaceFolder;
            var dest = await _files.OpenFolderAsync("Dossier où créer le projet", start);
            if (dest == null) return;

            var name = Selected.Id;
            var target = Path.Combine(dest, name);
            if (Directory.Exists(target)) { Status = $"Un dossier « {name} » existe déjà ici."; return; }

            try
            {
                await _templates.CreateMicroPythonProjectFromContentAsync(name, dest, src);
                _settings.AddRecentProject(target);
                await _dialogs.ShowMessageAsync("Projet créé",
                    $"« {name} » a été créé à partir de l'exemple (cadre inclus, prêt à tester).");
                _nav.NavigateToProjects();
            }
            catch (Exception ex) { Status = "Erreur : " + ex.Message; }
        }
    }
}
