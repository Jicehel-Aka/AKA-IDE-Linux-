namespace GamebuinoAKA.Core.Services
{
    public interface INavigationService
    {
        void NavigateToHome();
        void NavigateToProjects();
        void NavigateToNewProject();
        void NavigateToSettings();
        void NavigateToSpriteEditor();
        void NavigateToTilemapEditor();
    }
}
