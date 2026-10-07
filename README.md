# Gamebuino AKA IDE — Linux / Avalonia

IDE multiplateforme pour créer, gérer et compiler des projets Gamebuino AKA sur Linux, basé sur .NET 10 et Avalonia.

Ce dépôt contient une version Linux de l'IDE d'origine, avec un cœur métier portable et des outils de développement pour les projets Gamebuino AKA en PlatformIO et ESP-IDF.

## Vue d'ensemble

Le projet vise à fournir un environnement de développement complet pour la console Gamebuino AKA :

- création de projets Arduino / PlatformIO ou ESP-IDF ;
- gestion des assets graphiques et sonores ;
- éditeur de sprites et tilemaps ;
- génération de code C++ compatible avec la lib Gamebuino ;
- build, flash et monitor via les outils appropriés ;
- compatibilité Linux via les abstractions système et le portage Avalonia.

Le code est structuré pour séparer le cœur portable de l'interface graphique et des dépendances OS.

## Architecture du dépôt

```text
.
├── README.md                     # documentation principale
├── MODIFICATIONS.md              # historique des correctifs et évolutions
├── LOT-*.md                     # procédures et notes du projet
├── GamebuinoAKA.sln             # solution principale
├── GamebuinoAKA.IDE.sln         # solution IDE
├── AKA_procedures_alignees.zip  # archive de procédures/alignment
├── src/
│   ├── GamebuinoAKA.Core/       # logique métier portable
│   │   ├── Models/
│   │   ├── Platform/
│   │   ├── Services/
│   │   └── ...
│   └── GamebuinoAKA.App/        # application Avalonia / UI Linux
│       ├── Controls/
│       ├── Services/
│       ├── ViewModels/
│       ├── Views/
│       └── ...
├── tests/
│   └── GamebuinoAKA.Core.Tests/ # tests unitaires xUnit
└── .github/
    └── workflows/
```

## Règle d'architecture

Le cœur (`GamebuinoAKA.Core`) est conçu pour être portable et indépendant de l'interface graphique.

- il ne dépend pas de WPF, Avalonia, System.Drawing ou de chemins Windows-only ;
- les interactions système sont abstraites via des interfaces de plateforme ;
- les implémentations spécifiques à l'OS sont gérées dans l'application ou dans le code .NET portable pour les fonctionnalités natives.

Cette séparation permet de conserver une couche métier testable et réutilisable.

## Fonctionnalités principales

### 1. Gestion de projets

- scan du workspace ;
- détection automatique des projets PlatformIO et ESP-IDF ;
- création de nouveaux projets ;
- build / flash / monitor ;
- ouverture dans VS Code ou dans l'explorateur de fichiers ;
- travail avec les projets GitHub.

### 2. Support PlatformIO et ESP-IDF

Le projet prend en charge plusieurs chaînes de build :

- PlatformIO ;
- ESP-IDF ;
- sélection explicite ou auto-détection du système selon le projet.

Le système inclut également des mécanismes de détection plus robustes pour éviter les erreurs de routage entre PlatformIO et ESP-IDF.

### 3. Éditeur de sprites

- import d'images ou de spritesheets ;
- sélection et découpe libre ;
- réduction, recadrage, gestion des proportions ;
- transparence avec couleur clé ;
- export vers du code C++ compatible avec la lib Gamebuino ;
- format BGR565 (ordre utilisé par la librairie AKA) ;
- fichiers ré-éditables `.gbspr`.

### 4. Éditeur de tilemaps

- gestion des tilesets ;
- palette de tuiles ;
- calques de fond et premier plan ;
- peinture sur scène ;
- export C++ ;
- fichiers ré-éditables `.gbmap`.

### 5. Banque de sons

- import de fichiers audio ;
- organisation des effets et musiques ;
- lecture via le lecteur système ;
- intégration dans le workflow projet.

### 6. Procédures / snippets

- bibliothèque de blocs de code pour PlatformIO et ESP-IDF ;
- insertion directe dans les fichiers générés.

## Prérequis

- .NET SDK 10
- PlatformIO (`pio`) si vous souhaitez compiler / flasher des projets PlatformIO
- ESP-IDF (`idf.py`) si vous travaillez avec des projets ESP-IDF
- git (optionnel mais utile)
- VS Code (optionnel)
- sur Linux : membre du groupe `dialout` pour accéder aux ports série
  (`/dev/ttyUSB*`, `/dev/ttyACM*`)

## Démarrage rapide

```bash
dotnet restore GamebuinoAKA.sln
dotnet build GamebuinoAKA.sln -c Release
dotnet test GamebuinoAKA.sln -c Release
dotnet run --project src/GamebuinoAKA.App
```

## Points de portage importants

Le portage vers Linux et Avalonia a nécessité plusieurs adaptations :

- `System.Drawing` remplacé par `SkiaSharp` ;
- chemins Windows remplacés par les chemins XDG/Linux ;
- scripts Windows ESP-IDF adaptés à `export.sh` / `bash` ;
- lancement de processus sécurisé via `ArgumentList` ;
- audio hébergé par le lecteur système au lieu de `System.Media.SoundPlayer` ;
- MVVM migré vers `CommunityToolkit.Mvvm`.

## Historique et correctifs clés

Le fichier `MODIFICATIONS.md` documente les changements majeurs, notamment :

- support de PlatformIO et ESP-IDF côté projet ;
- correction du bug RGB565/BGR565 ;
- gestion de la transparence et de la clé de transparence ;
- amélioration des performances et sauvegarde ré-éditable (`.gbspr`, `.gbmap`) ;
- intégration d'un journal global de diagnostics et gestion robuste des erreurs ;
- amélioration du workflow de découpe et réduction de sprites ;
- auto-détection élargie des outils ESP-IDF et ports série ;
- correction des crashes sur la création de nouveaux projets.

## État du projet

Le cœur métier est porté et testé. L'interface Avalonia couvre les écrans et fonctionnalités essentielles, avec des améliorations continues sur les workflows d'édition, de build et de diagnostics.

La base technique est solide : le projet est bien organisé autour d'un moteur portable et d'une application UI indépendante des détails système.

## Licence

Voir le dépôt d'origine et les fichiers de licence associés dans le projet si disponibles.

## Documentation complémentaire

Ce dépôt contient plusieurs fichiers de procédures et notes, par exemple :

- `LOT-0-SOCLE.md`
- `LOT-1-2.md`
- `LOT-3.md`
- `LOT-4-5-6.md`
- `LOT-7-8-9.md`
- `LOT-10.md`
- `LOT-11-13.md`
- `LOT-14.md`
- `LOT-15.md`
- `LOT-ESPIDF-PROCEDURES.md`
- `LOT-PROCEDURES-ALIGNEES.md`
- `LOT-PROCEDURES-COMPLET.md`

Ces fichiers détaillent la logique de portage, les procédures de build et les mécanismes spécifiques au projet Gamebuino AKA.

## Remarques

- La solution principale est `GamebuinoAKA.sln`.
- La couche métier est testée via `GamebuinoAKA.Core.Tests`.
- L'application UI est portée sur Linux et se base sur Avalonia.
- Le référentiel est orienté vers la création d'un IDE complet et fiable pour Gamebuino AKA sur environnement Linux.

---

Ce README a été réécrit pour mieux refléter le projet réel, sa structure, ses fonctionnalités et le contexte technique documenté dans `MODIFICATIONS.md`.
