using System.Collections.Generic;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Catalogue des scripts d'exemple embarqués (galerie) : liste et lecture du source.
    /// </summary>
    public interface IExampleCatalogService
    {
        IReadOnlyList<ExampleInfo> GetExamples();

        /// <summary>Source complet d'un exemple, ou chaîne vide s'il est introuvable.</summary>
        string ReadSource(ExampleInfo example);
    }
}
