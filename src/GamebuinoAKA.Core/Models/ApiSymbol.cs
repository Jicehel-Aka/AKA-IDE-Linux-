namespace GamebuinoAKA.Core.Models
{
    /// <summary>
    /// Un symbole de l'API d'un langage AKA (fonction du module <c>aka</c>, constante
    /// de bouton, fonction/rappel Love2D…), servant l'autocomplétion et l'aide au survol
    /// dans l'éditeur de scripts. Chargé depuis les catalogues JSON embarqués
    /// (<c>Api/micropython-aka.json</c>, <c>Api/love2d-aka.json</c>).
    /// </summary>
    public sealed class ApiSymbol
    {
        /// <summary>Nom inséré à la complétion (ex. <c>fill_rect</c>, <c>love.graphics.circle</c>).</summary>
        public string Name { get; set; } = "";

        /// <summary>Signature lisible affichée dans l'aide (ex. <c>aka.fill_rect(x, y, w, h)</c>).</summary>
        public string Signature { get; set; } = "";

        /// <summary>Catégorie : function | constant | callback | module.</summary>
        public string Kind { get; set; } = "function";

        /// <summary>Description courte (une à deux phrases).</summary>
        public string Doc { get; set; } = "";
    }
}
