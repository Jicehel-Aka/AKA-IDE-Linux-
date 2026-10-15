using System.Text.RegularExpressions;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Détecte les emplacements d'erreur dans une ligne de sortie console.
    /// Gère le format traceback MicroPython (identique sur le simulateur et sur la
    /// carte) : <c>  File "pong.py", line 42, in update</c>, y compris
    /// <c>File "&lt;stdin&gt;", line N</c> pour le code collé au REPL.
    /// </summary>
    public static class TracebackParser
    {
        // Ex. : File "jeu.py", line 42, in <module>
        private static readonly Regex FileLine = new(
            "File\\s+\"(?<f>[^\"]+)\",\\s*line\\s+(?<l>\\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Renvoie l'emplacement si la ligne en contient un, sinon <c>null</c>.
        /// </summary>
        public static ErrorLocation? ParseLine(string? line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            var m = FileLine.Match(line);
            if (!m.Success) return null;
            if (!int.TryParse(m.Groups["l"].Value, out var ln) || ln <= 0) return null;
            return new ErrorLocation
            {
                File = m.Groups["f"].Value,
                Line = ln,
                Raw = line.Trim()
            };
        }
    }
}
