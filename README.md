# 🎮 Gamebuino AKA IDE — Édition Multiplateforme (Linux & Windows)

[![GitHub Release](https://img.shields.io/github/v/release/Jicehel-Aka/AKA-IDE-Linux-?color=emerald&label=Release%20Officielle)](https://github.com/Jicehel-Aka/AKA-IDE-Linux-/releases)
[![Build, Test & Release Multiplatform](https://github.com/Jicehel-Aka/AKA-IDE-Linux-/actions/workflows/ci.yml/badge.svg)](https://github.com/Jicehel-Aka/AKA-IDE-Linux-/actions)
[![Platform](https://img.shields.io/badge/Platform-Linux%20%7C%20Windows-blue.svg)](https://dotnet.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![UI](https://img.shields.io/badge/UI-Avalonia%2011.2-orange.svg)](https://avaloniaui.net/)
[![Licence](https://img.shields.io/badge/Licence-GPL--3.0-green.svg)](LICENSE)

Environnement de développement intégré complet (**C# .NET 10 + Avalonia UI**) pour concevoir, programmer, simuler et flasher des jeux sur la console portable rétro **Gamebuino AKA** (ESP32-S3, écran couleur 320×240, audio, LEDs NeoPixels et retour haptique).

---

## 🚀 Fonctionnalités Principales

### 🛠️ Gestion Multi-Langages & Chaînes de Build
- ⚙️ **C++ Natif (PlatformIO & ESP-IDF)** : Compilation optimisée 60 FPS, flash USB automatique sans root sous Linux, et moniteur série interactif.
- 🐍 **MicroPython** : Création de scripts Python (`main.py`), intégration du module `aka`, transfert direct vers la mémoire flash via `akatransfer.py`.
- 🕹️ **Love2D (Lua)** : Prototypage rapide de mécaniques 2D avec le moteur Love2D.
- 💻 **Simulateurs PC Autonomes** : Exécution et débogage instantanés sur ordinateur sans console physique grâce aux simulateurs embarqués (`aka-py-sim` et `akalove-play`).

### 🎨 Suite d'Éditeurs Visuels Intégrés
- 🎨 **Éditeur de Sprites** : Découpage libre de planches au glisser, recadrage, réduction d'échelle, couleur-clé de transparence et conversion C++ en format couleur natif **BGR565** (`.gbspr`).
- 🗺️ **Éditeur de Tilemaps** : Palette de tuiles découpées, calques multiples (fond et premier plan), grille de peinture et export de projets (`.gbmap`).
- 🔊 **Banque de Sons** : Écoute et gestion de bruitages et thèmes musicaux (.wav, .pmf, en-têtes C++).
- 🧩 **Bibliothèque de Snippets (53 procédures)** : Formes géométriques, physique (gravité, rebond, vélocité), entrées D-Pad et boutons, timers et scores.

### ✍️ Éditeur de Scripts & Débogage
- 💡 **Autocomplétion & aide au survol** : Signatures et documentation du module `aka` (MicroPython) et de l'API Love2D, à la frappe (ou Ctrl-Espace) et au survol.
- 🖥️ **Console / Moniteur série** : Sortie console de l'AKA réelle sur le PC (REPL MicroPython, `print()`, logs), choix du port et du débit, renvoi de commandes (Entrée, Ctrl-C).
- 🔗 **Erreurs cliquables** : Les tracebacks du simulateur ou de la carte deviennent cliquables et sautent directement à la ligne fautive dans l'éditeur.
- ⚡ **REPL interactif** : Exécuter le script courant sur la carte via le raw-REPL, sans écrire de fichier, pour une boucle d'essai quasi instantanée.
- 📚 **Galerie d'exemples** : 10 jeux prêts à l'emploi (Pong, Snake, Tetris, Calculatrice…) à ouvrir dans l'éditeur ou à transformer en projet (cadre inclus).

### 🧱 Studio Visuel Blockly & Tutoriels
- 🧱 **Programmation par Blocs** : Conception visuelle accessible aux débutants avec double export automatique en **MicroPython** et **C++ Arduino**.
- 📚 **9 Parcours Guidés Multilingues (FR, EN, DE, IT, PT, ES)** :
  1. 🏓 Pong Rétro Complet
  2. 🚀 Shoot'em Up Spatial
  3. 🏃 Jeu de Plateforme avec Gravité
  4. 🟡 Pac-Maze Labyrinthe
  5. 🧮 Calculatrice Rétro D-Pad
  6. 🐾 Animal Virtuel / Tamagotchi
  7. 🎹 Synthétiseur Chiptune & Boîte à Rythmes
  8. ⏱️ Chrono & Minuteur Pomodoro
  9. ⏳ Minuteur Cuisson des Œufs & Jeux de Société

---

## 📦 Téléchargements (Release v2.4.0)

Rendez-vous dans la section [Releases](https://github.com/Jicehel-Aka/AKA-IDE-Linux-/releases/tag/v2.4.0) pour télécharger les binaires autonomes :
- 🐧 **Linux (x64)** : `GamebuinoAKA-IDE-linux-x64.tar.gz`
- 🪟 **Windows (x64)** : `GamebuinoAKA-IDE-windows-x64.zip`
- 🔒 **Empreintes de sécurité** : `SHA256SUMS.txt`

### 🐧 Démarrage Rapide sous Linux
```bash
tar -xzf GamebuinoAKA-IDE-linux-x64.tar.gz
./GamebuinoAKA.App
```
*Pour flasher l'ESP32-S3 sans accès root, copiez `packaging/linux/99-gamebuino.rules` dans `/etc/udev/rules.d/` et rechargez udev.*

---

## 🛠️ Compilation & Tests depuis les Sources

### Prérequis
- **.NET 10 SDK**
- Pour le flash C++ : **PlatformIO** (`pio`) ou **ESP-IDF** (`idf.py`).

```bash
# 1. Cloner le dépôt
git clone -b avalonia https://github.com/Jicehel-Aka/AKA-IDE-Linux-.git
cd AKA-IDE-Linux-

# 2. Restaurer et compiler
dotnet restore GamebuinoAKA.sln
dotnet build GamebuinoAKA.sln --configuration Release

# 3. Lancer la suite de tests unitaires
dotnet test GamebuinoAKA.sln --configuration Release

# 4. Lancer l'IDE
dotnet run --project src/GamebuinoAKA.App
```


---

## 🏗️ Architecture du projet

```text
GamebuinoAKA/
├── .github/workflows/ci.yml       ← CI multiplateforme Linux & Windows
├── docs/                          ← Documentation d'architecture et historique des lots
├── packaging/linux/               ← Règles udev et fichier .desktop
├── src/
│   ├── GamebuinoAKA.Core/         ← Cœur métier portable (100% découplé de l'UI)
│   └── GamebuinoAKA.App/          ← Interface graphique Avalonia UI (XAML)
└── tests/
    └── GamebuinoAKA.Core.Tests/   ← Suite de tests xUnit (sans UI)
```
