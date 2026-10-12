namespace GamebuinoAKA.Core.Models
{
    /// <summary>
    /// Nature d'un projet AKA, au-delà de la simple chaîne de build native.
    /// Les deux derniers sont des projets « script » exécutés par un interpréteur
    /// embarqué sur la console (et simulés sur PC).
    /// </summary>
    public enum ProjectKind
    {
        PlatformIO,     // jeu natif Arduino
        EspIdfNative,   // jeu natif ESP-IDF (coquille)
        MicroPython,    // script .py (interpréteur MicroPython-AKA)
        Love2D          // jeu .lua (runtime AKA-Love)
    }
}
