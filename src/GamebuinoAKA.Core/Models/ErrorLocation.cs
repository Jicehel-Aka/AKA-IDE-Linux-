namespace GamebuinoAKA.Core.Models
{
    /// <summary>
    /// Emplacement d'erreur extrait d'une sortie console (traceback MicroPython,
    /// message du simulateur, log de la carte) : fichier + numéro de ligne.
    /// </summary>
    public sealed class ErrorLocation
    {
        public string File { get; set; } = "";
        public int Line { get; set; }

        /// <summary>Texte d'origine de la ligne (pour l'affichage).</summary>
        public string Raw { get; set; } = "";

        public override string ToString() => $"{File}:{Line}";
    }
}
