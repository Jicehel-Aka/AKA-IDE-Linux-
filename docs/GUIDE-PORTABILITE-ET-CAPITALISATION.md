# Guide de Portabilité & Capitalisation Technique — Gamebuino AKA IDE

Ce document formalise les écueils rencontrés lors du portage de l'IDE depuis **WPF (.NET Windows)** vers **Avalonia UI (.NET 10 multiplateforme Linux & Windows)**, les solutions architecturales adoptées et la procédure de contrôle qualité à appliquer pour tout développement futur.

---

## 1. Registre des Écueils Rencontrés & Solutions Systémiques

### ❌ Écueil n°1 : Chemins Windows avec antislashs (`\`) dans les fichiers de solution et projets
- **Symptôme** : Erreur `MSB3202: The project file ... was not found` sur les runners Linux (Ubuntu).
- **Cause racine** : Dans `GamebuinoAKA.sln` et les `<ProjectReference>`, les chemins étaient écrits avec des antislashs (`src\GamebuinoAKA.Core\...`). Sous Linux, l'antislash est un caractère littéral valide dans un nom de fichier et **non** un séparateur de dossier. Le compilateur cherchait donc un fichier unique nommé `src\...\...csproj` à la racine au lieu d'entrer dans les sous-dossiers.
- **Règle absolue** : **Toujours utiliser des barres obliques (`/`)** dans les fichiers `.sln` et `.csproj`. Le format slash est universellement reconnu par Windows, macOS et Linux.

---

### ❌ Écueil n°2 : Encapsulation de code source dans des archives ZIP au sein de Git
- **Symptôme** : Fichiers indispensables non trouvés (`CS0246`) après clonage ou extraction.
- **Cause racine** : Des portions entières de code (notamment les services Core et les snippets) avaient été committées sous forme d'archive compressée (`AKA_procedures_alignees.zip`). Git ne peut pas indexer, fusionner ni compiler le contenu d'un ZIP sans étape d'extraction explicite.
- **Règle absolue** : **Zéro fichier d'archive (.zip, .tar, .7z) dans l'arborescence des sources**. Tout code doit être déposé en texte clair dans l'arborescence standard (`src/`, `tests/`).

---

### ❌ Écueil n°3 : Rupture de dépendance dans la racine de composition (Bootstrapper / DI)
- **Symptôme** : `error CS0103: The name 'Bootstrapper' does not exist in the current context` et `error CS0246: The type or namespace name '...' could not be found`.
- **Cause racine** : Décalage entre les signatures requises par l'interface graphique (`MainViewModel`, `SoundBankViewModel`, `SpriteEditorViewModel`) et les classes concrètes fournies par le Core (`AssetService`, `CodeSnippetService`, `SoundBankService`).
- **Règle absolue** : Tout service consommé par un ViewModel doit obligatoirement :
  1. Posséder son contrat d'interface (`IService`) dans `src/GamebuinoAKA.Core/Services/`.
  2. Être implémenté dans `GamebuinoAKA.Core`.
  3. Être instancié et injecté dans `src/GamebuinoAKA.App/Bootstrapper.cs`.
  4. Être validé par un test unitaire dédié dans `tests/GamebuinoAKA.Core.Tests/`.

---

### ❌ Écueil n°4 : Dépendances graphiques non portables (GDI+ / System.Drawing)
- **Symptôme** : Crash à l'exécution ou en CI headless sous Linux avec `TypeInitializationException` sur `System.Drawing`.
- **Cause racine** : `System.Drawing.Common` n'est plus supporté sous Linux depuis .NET 7.
- **Règle adoptée** : Migration complète vers **SkiaSharp** (`SKBitmap`, `SKImage`) avec le paquet `SkiaSharp.NativeAssets.Linux.NoDependencies` pour garantir l'exécution sans serveur X11 ni police fontconfig lors des tests unitaires en CI.

---

### ❌ Écueil n°5 : Respect des normes Linux FreeDesktop & Droits matériels
- **Problème** : Fichiers de configuration enregistrés dans le dossier d'installation, IDE absent des menus d'applications, et blocage d'accès au port série USB de la Gamebuino sans `sudo`.
- **Règles adoptées** :
  - **Spécification XDG Base Directory** : Utilisation de `$XDG_CONFIG_HOME` (`~/.config/GamebuinoAKA`) et `$XDG_DATA_HOME` (`~/.local/share/GamebuinoAKA`) via `LinuxPlatformPaths.cs`.
  - **FreeDesktop** : Intégration du fichier `packaging/linux/gamebuino-aka.desktop`.
  - **Règles udev USB** : Fourniture de `packaging/linux/99-gamebuino.rules` attribuant les consoles au groupe `dialout` avec permissions `0666`.

---

## 2. Matrice de Dépendance des Services (Core & App)

| Service | Interface | Fichier source | Rôle & Dépendances |
| :--- | :--- | :--- | :--- |
| **AssetService** | — (classe concrète) | `src/GamebuinoAKA.Core/Services/AssetService.cs` | Pipeline SkiaSharp, conversion BGR565, rognage, export C++ sprites & tilemaps. |
| **CodeSnippetService** | `ICodeSnippetService` | `src/GamebuinoAKA.Core/Services/CodeSnippetService.cs` | Catalogue des 53 snippets PlatformIO & ESP-IDF, injection dans le squelette C++. |
| **SoundBankService** | `ISoundBankService` | `src/GamebuinoAKA.Core/Services/SoundBankService.cs` | Scan des fichiers audio (.wav, .pmf, .h), catégorisation et export. |
| **PlatformIOService** | `IPlatformIOService` | `src/GamebuinoAKA.Core/Services/PlatformIOService.cs` | Détection et pilotage de `pio` (run, upload, monitor). |
| **EspIdfService** | `IEspIdfService` | `src/GamebuinoAKA.Core/Services/EspIdfService.cs` | Pilotage d'ESP-IDF via script d'environnement (`export.sh` / `export.bat`). |
| **BuildService** | `IBuildService` | `src/GamebuinoAKA.Core/Services/BuildService.cs` | Aiguilleur universel routant vers PlatformIO ou ESP-IDF selon le projet. |
| **SettingsService** | `ISettingsService` | `src/GamebuinoAKA.Core/Services/SettingsService.cs` | Sauvegarde et chargement des préférences utilisateur selon `IPlatformPaths`. |
| **GitService** | `IGitService` | `src/GamebuinoAKA.Core/Services/GitService.cs` | Clonage de dépôts GitHub sans shell intermédiaire. |
| **FileLogService** | `ILogService` | `src/GamebuinoAKA.Core/Services/FileLogService.cs` | Journalisation fichier thread-safe avec rotation automatique (.old). |

---

## 3. Procédure de Contrôle Qualité avant tout Commit

Avant de pousser une modification sur GitHub :

```bash
# 1. Vérification de l'arborescence (aucun binaire ni zip commité)
git status --ignored

# 2. Restauration propre et compilation en mode Release
dotnet restore GamebuinoAKA.sln
dotnet build GamebuinoAKA.sln --configuration Release --no-restore

# 3. Exécution obligatoire de la suite de tests unitaires
dotnet test GamebuinoAKA.sln --configuration Release --no-build

# 4. Publication de test sous Linux (si vous êtes sur Linux)
dotnet publish src/GamebuinoAKA.App/GamebuinoAKA.App.csproj -c Release -r linux-x64 --self-contained
```

---

## 4. Structure de Référence d'un Dépôt Avalonia Multiplateforme

```text
GamebuinoAKA/
├── .github/workflows/ci.yml       # Matrice de build Ubuntu + Windows
├── .gitignore                     # Exclusion de bin/, obj/, artifacts/, *.pdb
├── GamebuinoAKA.sln               # Solution unique avec barres obliques (/)
├── Makefile                       # Cible d'installation sous Linux
├── README.md                      # Documentation d'accueil et badges CI
├── docs/                          # Architecture et capitalisation
│   ├── ARCHITECTURE.md
│   ├── GUIDE-PORTABILITE-ET-CAPITALISATION.md
│   └── migration-lots/            # Historique des lots de portage
├── packaging/linux/               # Intégration OS Linux (udev, .desktop)
│   ├── 99-gamebuino.rules
│   └── gamebuino-aka.desktop
├── src/
│   ├── GamebuinoAKA.Core/         # Cœur métier 100% découplé
│   │   ├── Models/
│   │   ├── Platform/
│   │   └── Services/
│   └── GamebuinoAKA.App/          # Interface Avalonia UI
│       ├── Controls/
│       ├── Platform/
│       ├── Services/
│       ├── ViewModels/
│       └── Views/
└── tests/
    └── GamebuinoAKA.Core.Tests/   # Tests xUnit headless
```
