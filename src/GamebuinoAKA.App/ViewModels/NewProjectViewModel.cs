using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;

namespace GamebuinoAKA.App.ViewModels
{
    public sealed partial class NewProjectViewModel : ViewModelBase
    {
        private readonly ITemplateService _templates;
        private readonly ISettingsService _settings;
        private readonly INavigationService _nav;
        private readonly IFileDialogService _files;
        private readonly IDialogService _dialogs;

        // Libellés des types de projet (l'ordre fixe les préfixes testés plus bas).
        public const string TypeEspIdf = "ESP-IDF (coquille)";
        public const string TypePlatformIO = "PlatformIO (Arduino)";
        public const string TypeMicroPython = "MicroPython (script .py)";

        [ObservableProperty] private string _projectName = string.Empty;
        [ObservableProperty] private string _destinationFolder = string.Empty;
        [ObservableProperty] private string _selectedProjectType = TypeEspIdf;
        [ObservableProperty] private string _selectedTemplate = "empty";
        [ObservableProperty] private bool _addAudio;
        [ObservableProperty] private string _status = string.Empty;

        public string[] ProjectTypes { get; } = new[] { TypeEspIdf, TypePlatformIO, TypeMicroPython };

        public bool IsEspIdf => SelectedProjectType == TypeEspIdf;
        public bool IsPlatformIO => SelectedProjectType == TypePlatformIO;
        public bool IsMicroPython => SelectedProjectType == TypeMicroPython;

        public string[] Templates { get; } = new[] { "empty", "hello-world", "game-template" };

        public NewProjectViewModel(ITemplateService templates, ISettingsService settings,
            INavigationService nav, IFileDialogService files, IDialogService dialogs)
        {
            _templates = templates; _settings = settings; _nav = nav; _files = files; _dialogs = dialogs;
            _destinationFolder = settings.Settings.WorkspaceFolder;
            _selectedProjectType = settings.Settings.DefaultBuildSystem == BuildSystem.PlatformIO
                ? TypePlatformIO : TypeEspIdf;
            _selectedTemplate = IsPlatformIO ? "empty" : "esp-idf";
        }

        partial void OnSelectedProjectTypeChanged(string value)
        {
            OnPropertyChanged(nameof(IsEspIdf));
            OnPropertyChanged(nameof(IsPlatformIO));
            OnPropertyChanged(nameof(IsMicroPython));
            if (IsEspIdf) SelectedTemplate = "esp-idf";
            else if (IsPlatformIO) { SelectedTemplate = "empty"; AddAudio = false; }
            else { AddAudio = false; }   // MicroPython : pas de template natif ni d'audio ESP-IDF
        }

        [RelayCommand]
        private async Task Browse()
        {
            var f = await _files.OpenFolderAsync("Dossier de destination", DestinationFolder);
            if (f != null) DestinationFolder = f;
        }

        [RelayCommand]
        private async Task Create()
        {
            Status = string.Empty;
            if (string.IsNullOrWhiteSpace(ProjectName)) { Status = "Le nom du projet est requis."; return; }
            if (string.IsNullOrWhiteSpace(DestinationFolder)) { Status = "Le dossier de destination est requis."; return; }

            var target = Path.Combine(DestinationFolder, ProjectName);
            if (Directory.Exists(target)) { Status = $"Un dossier « {ProjectName} » existe déjà."; return; }

            try
            {
                if (IsMicroPython)
                {
                    await _templates.CreateMicroPythonProjectAsync(ProjectName, DestinationFolder);
                }
                else
                {
                    var bs = IsEspIdf ? BuildSystem.EspIdf : BuildSystem.PlatformIO;
                    await _templates.CreateProjectAsync(ProjectName, SelectedTemplate, DestinationFolder, bs, IsEspIdf && AddAudio);
                }
                _settings.AddRecentProject(target);
                await _dialogs.ShowMessageAsync("Projet créé", $"« {ProjectName} » a été créé.");
                _nav.NavigateToProjects();
            }
            catch (Exception ex) { Status = "Erreur : " + ex.Message; }
        }
    }
}
