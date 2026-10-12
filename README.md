# Gamebuino AKA IDE — Édition Multiplateforme (Linux & Windows)

[![Build & Test](https://github.com/Jicehel-Aka/AKA-IDE-Linux-/actions/workflows/ci.yml/badge.svg)](https://github.com/Jicehel-Aka/AKA-IDE-Linux-/actions)
[![Platform](https://img.shields.io/badge/Platform-Linux%20%7C%20Windows-blue.svg)](https://dotnet.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![UI](https://img.shields.io/badge/UI-Avalonia%2011-orange.svg)](https://avaloniaui.net/)

IDE complet (.NET 10 + Avalonia UI) pour créer, programmer et flasher des jeux **Gamebuino AKA** (ESP32-S3), compatible avec **PlatformIO** et **ESP-IDF**.

---

## 🚀 Fonctionnalités principales

- 📁 **Gestionnaire de projets** : Scan automatique, création de projets (modèles PlatformIO Arduino ou ESP-IDF), compilation, flash et moniteur série en direct.
- 🎨 **Éditeur de sprites** : Import d'images/planches, découpage libre au glisser, recadrage, réduction d'échelle, transparence par couleur-clé et export C++ déterministe en format couleur **BGR565** (conforme à la bibliothèque matérielle Gamebuino AKA). Formats ré-éditables `.gbspr`.
- 🗺️ **Éditeur de tilemaps** : Palette de tuiles découpées, calques fond/premier plan, peinture directe, export C++ et projets `.gbmap`.
- 🔊 **Banque de sons** : Détection et écoute de sons (.wav, .pmf, en-têtes C++), organisation par thèmes.
- 🧩 **Bibliothèque de snippets (53 procédures)** : Catalogue complet pour **PlatformIO (32)** et **ESP-IDF (21)**, injectables directement dans le code du jeu (formes, textes, entrées, gravité, vélocité, rebond, scores, timers, etc.).
- 🐧 **Intégration Linux native** : Respect des répertoires standards XDG (`~/.config/GamebuinoAKA`, `~/.local/share/GamebuinoAKA`), règles udev USB pour le flash sans `sudo`, et fichier `.desktop`.

---

## 📦 Prérequis

- **.NET SDK 10**
- Pour compiler/flasher les jeux : **PlatformIO** (`pio`) et/ou **ESP-IDF** (`idf.py`).
- Sous Linux : ajouter votre utilisateur au groupe `dialout` pour l'accès aux ports série :
  ```bash
  sudo usermod -aG dialout $USER
  ```
  Et installer les règles udev fournies dans `packaging/linux/99-gamebuino.rules`.

---

## 🛠️ Compilation & Tests

```bash
# 1. Restaurer les dépendances
dotnet restore GamebuinoAKA.sln

# 2. Compiler la solution
dotnet build GamebuinoAKA.sln --configuration Release

# 3. Lancer la suite de tests (80 tests unitaires)
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
