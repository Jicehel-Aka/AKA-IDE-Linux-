using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamebuinoAKA.Core.Platform;
using GamebuinoAKA.Core.Services;

namespace GamebuinoAKA.App.ViewModels
{
    public sealed partial class MainViewModel : ViewModelBase, INavigationService
    {
        public HomeViewModel Home { get; }
        public ProjectsViewModel Projects { get; }
        public NewProjectViewModel NewProject { get; }
        public SettingsViewModel Settings { get; }
        public SpriteEditorViewModel SpriteEditor { get; }

        private readonly IDialogService _dialogs;

        [ObservableProperty] private object? _currentPage;

        public MainViewModel(
            ISettingsService settings, IProjectService projects, ITemplateService templates,
            IBuildService build, IGitService git, IVSCodeService vscode,
            IApplicationLauncher launcher, IPlatformIOService pio, IEspIdfService idf,
            AssetService asset, IFileDialogService files, IDialogService dialogs)
        {
            _dialogs = dialogs;

            Home = new HomeViewModel(this);
            Projects = new ProjectsViewModel(projects, build, git, vscode, launcher, settings, this, dialogs);
            NewProject = new NewProjectViewModel(templates, settings, this, files, dialogs);
            Settings = new SettingsViewModel(settings, pio, idf, vscode, files);
            SpriteEditor = new SpriteEditorViewModel(asset, settings, files, dialogs);

            NavigateToProjects();
        }

        [RelayCommand] public void NavigateToHome() => CurrentPage = Home;
        [RelayCommand] public void NavigateToProjects() { _ = Projects.RefreshAsync(); CurrentPage = Projects; }
        [RelayCommand] public void NavigateToNewProject() => CurrentPage = NewProject;
        [RelayCommand] public void NavigateToSettings() => CurrentPage = Settings;
        [RelayCommand] public void NavigateToSpriteEditor() => CurrentPage = SpriteEditor;

        public async void NavigateToTilemapEditor()
            => await _dialogs.ShowMessageAsync("Éditeur de tilemaps", "Disponible dans un prochain lot (14).");
    }
}
