namespace GamebuinoAKA.Core.Models
{
    /// <summary>
    /// Un exemple de la galerie : métadonnées (titre, description, langage) et chemin
    /// relatif du script embarqué sous <c>Examples/</c>.
    /// </summary>
    public sealed class ExampleInfo
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Language { get; set; } = "MicroPython";
        /// <summary>Chemin relatif sous Examples/ (ex. <c>py/pong.py</c>).</summary>
        public string File { get; set; } = "";
    }
}
