using System.Collections.Generic;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Fournit la liste des symboles d'API (fonctions, constantes, rappels) d'un
    /// langage de script AKA, pour l'autocomplétion et l'aide au survol de l'éditeur.
    /// </summary>
    public interface IApiCatalogService
    {
        /// <summary>Symboles du langage correspondant au type de projet (vide si non-script).</summary>
        IReadOnlyList<ApiSymbol> GetSymbols(ProjectKind kind);
    }
}
