using GamebuinoAKA.Core.Models;

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

        /// <summary>Ouvre l'éditeur de scripts sur le fichier donné, positionné à la ligne
        /// indiquée (utilisé par les erreurs cliquables du simulateur et du moniteur série).</summary>
        void OpenScriptAt(string file, int line);

        /// <summary>Charge un contenu de script dans l'éditeur (non enregistré) et s'y rend
        /// (utilisé par la galerie d'exemples : « Ouvrir dans l'éditeur »).</summary>
        void OpenScriptContent(string suggestedName, string text, ProjectKind kind);
    }
}
