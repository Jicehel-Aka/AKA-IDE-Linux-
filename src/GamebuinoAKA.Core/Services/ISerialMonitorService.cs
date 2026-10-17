using System;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Moniteur série : lit la sortie console de l'AKA réelle (REPL MicroPython,
    /// print() d'un jeu Love2D/Python, logs du firmware natif) via le port USB-série
    /// et permet d'y renvoyer des caractères (Ctrl-C, Ctrl-D, commandes REPL…).
    ///
    /// Symétrique du simulateur PC : le simulateur montre la console d'un script qui
    /// tourne sur le PC ; ce moniteur montre la console d'un programme qui tourne sur
    /// la carte. Multiplateforme (Linux /dev/tty* et Windows COMx) via System.IO.Ports.
    /// </summary>
    public interface ISerialMonitorService
    {
        bool IsOpen { get; }
        string? CurrentPort { get; }

        /// <summary>Ports série disponibles (COMx / /dev/tty*).</summary>
        string[] ListPorts();

        /// <summary>Texte reçu de la carte (déjà décodé). Émis hors thread UI.</summary>
        event Action<string>? DataReceived;

        /// <summary>Événements de cycle de vie / erreurs lisibles (connexion, fermeture…).</summary>
        event Action<string>? StatusChanged;

        /// <summary>Ouvre le port (ferme d'abord un éventuel port déjà ouvert).</summary>
        void Open(string port, int baudRate);

        /// <summary>Ferme le port s'il est ouvert.</summary>
        void Close();

        /// <summary>Envoie du texte brut vers la carte (sans ajout de fin de ligne).</summary>
        void Send(string text);

        /// <summary>Exécute du code MicroPython sur la carte via le raw-REPL (sans écrire
        /// de fichier) : interrompt, bascule en raw REPL, envoie le code et l'exécute.
        /// La sortie revient par <see cref="DataReceived"/>.</summary>
        void RunViaRawRepl(string code);
    }
}
