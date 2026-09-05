Oui : le README actuel décrit la version historique WPF/Windows. Dans le dépôt Linux, il faut le remplacer par une documentation qui :

- présente le portage Avalonia ;
- indique clairement que la migration est en cours ;
- ne promet pas encore toutes les fonctions si elles ne sont pas migrées ;
- documente Linux, PlatformIO CLI et GitHub Actions ;
- conserve le lien avec `legacy-wpf/`.

Voici un `README.md` prêt à utiliser pour la branche `avalonia`.

---

## Remplacer le README

Depuis :

```bash
cd ~/.bob/playground/GamebuinoAKA.IDE_Linux
git checkout avalonia
nano README.md
```

Supprime le contenu existant et colle ceci :

````markdown
# Gamebuino AKA IDE — Avalonia Edition

[![Build and test Linux](https://github.com/Jicehel-Aka/AKA-IDE-Linux-/actions/workflows/linux.yml/badge.svg?branch=avalonia)](https://github.com/Jicehel-Aka/AKA-IDE-Linux-/actions/workflows/linux.yml)

Version multiplateforme en cours de développement de **Gamebuino AKA IDE**.

Gamebuino AKA IDE est un launcher et un ensemble d’outils pour faciliter le développement de jeux destinés à la console **Gamebuino AKA**, basée sur un ESP32-S3 avec écran 320×240.

> [!WARNING]
> Ce dépôt correspond au portage en cours de l’ancienne application Windows WPF vers **.NET 10 + Avalonia UI**.
>
> Certaines fonctionnalités historiques peuvent être encore en cours de migration.  
> La version WPF de référence est conservée dans [`legacy-wpf/`](legacy-wpf/).

---

## Objectif du projet

L’application ne cherche pas à remplacer :

- VS Code ;
- PlatformIO ;
- Git ;
- esptool ;
- la toolchain ESP32.

Elle fournit une interface simplifiée autour de ces outils, ainsi que des éditeurs d’assets Gamebuino AKA.

```text
Gamebuino AKA IDE
        │
        ├── Gestion de projets et templates
        ├── Git
        ├── VS Code
        ├── PlatformIO
        │   ├── Build
        │   ├── Upload / Flash
        │   └── Serial Monitor
        │
        ├── Éditeur de sprites
        ├── Éditeur de tilemaps
        ├── Banque de sons
        └── Snippets C++
```

---

## Technologies

| Élément | Technologie |
|---|---|
| Langage | C# |
| Runtime | .NET 10 (`net10.0`) |
| Interface graphique | Avalonia UI |
| Architecture | MVVM |
| MVVM | CommunityToolkit.Mvvm |
| Injection de dépendances | Microsoft.Extensions.DependencyInjection |
| Build firmware | PlatformIO CLI |
| Flash ESP32-S3 | PlatformIO / esptool |
| Tests | xUnit |
| CI Linux | GitHub Actions Ubuntu |

---

## Plateformes visées

| Plateforme | État |
|---|---|
| Linux x64 | Cible prioritaire |
| Windows | Compatible via Avalonia |
| macOS | Cible possible à terme |

La nouvelle version ne dépend pas de WPF et ne nécessite pas Wine sous Linux.

---

## Architecture

```text
AKA-IDE-Linux-/
│
├── src/
│   ├── GamebuinoAKA.Core/
│   │   ├── Assets/
│   │   ├── Graphics/
│   │   ├── Models/
│   │   ├── Platform/
│   │   ├── Services/
│   │   └── ViewModels/
│   │
│   └── GamebuinoAKA.App/
│       ├── Controls/
│       ├── Platform/
│       ├── Themes/
│       ├── Views/
│       ├── App.axaml
│       └── MainWindow.axaml
│
├── tests/
│   └── GamebuinoAKA.Core.Tests/
│
├── legacy-wpf/
│   └── src/
│       └── GamebuinoAKA.IDE/
│
├── .github/
│   └── workflows/
│       └── linux.yml
│
└── GamebuinoAKA.sln
```

### Règles d’architecture

`GamebuinoAKA.Core` contient le métier portable et ne doit dépendre ni de WPF ni d’Avalonia.

Le projet `Core` ne doit pas référencer :

```text
System.Windows
PresentationFramework
PresentationCore
WindowsBase
System.Drawing.Common
explorer.exe
Code.exe
pio.exe
%APPDATA%
```

Les détails spécifiques au système d’exploitation sont isolés dans :

```text
GamebuinoAKA.App/Platform/
```

Exemples :

| Fonction | Windows | Linux |
|---|---|---|
| Ouvrir un dossier | `explorer.exe` | `xdg-open` |
| Lancer VS Code | `code.exe` / `code.cmd` | `code` |
| Git | `git.exe` | `git` |
| PlatformIO | `pio.exe` | `pio` / `platformio` |
| Ports série | `COM3` | `/dev/ttyACM0`, `/dev/ttyUSB0` |
| Configuration | `%APPDATA%` | XDG / `~/.config` |

---

## Branches importantes

| Branche / tag | Rôle |
|---|---|
| `main` | Référence historique de la version WPF |
| `v1-wpf-final` | Tag de sauvegarde de la dernière version WPF |
| `avalonia` | Développement du portage .NET 10 / Avalonia / Linux |
| `github-initial` | Sauvegarde éventuelle de l’état initial du dépôt GitHub |

Le développement du portage doit être effectué sur :

```bash
git checkout avalonia
```

---

## Fonctionnalités visées

| Fonctionnalité | État cible |
|---|---|
| Création de projets depuis templates | Prévue |
| Gestion de projets récents | Prévue |
| Clonage de dépôts GitHub | Prévu |
| Lancement de VS Code | Prévu |
| Build PlatformIO | Prévu |
| Flash / Upload PlatformIO | Prévu |
| Moniteur série PlatformIO | Prévu |
| Éditeur de sprites | Prévu |
| Conversion BGR565 AKA | En cours |
| Export de sprites C++ | Prévu |
| Éditeur de tilemaps | Prévu |
| Export de tilemaps C++ | Prévu |
| Banque de sons | Prévue |
| Snippets C++ | Prévu |

Les fonctions firmware restent déléguées à PlatformIO :

```text
Build   → pio run
Flash   → pio run -t upload
Monitor → pio device monitor
```

---

## Configuration PlatformIO Gamebuino AKA

Les templates Gamebuino AKA utilisent une configuration PlatformIO semblable à celle-ci :

```ini
[env:gamebuino_aka]
platform = espressif32
board = esp32-s3-devkitc-1
framework = arduino

monitor_speed = 115200

board_build.flash_size = 16MB
board_build.psram_type = opi

lib_deps =
    https://github.com/jmp42/Gamebuino_AKA_lib

build_flags =
    -DBOARD_HAS_PSRAM
    -mfix-esp32-psram-cache-issue

upload_protocol = esptool
```

PlatformIO télécharge automatiquement la toolchain Xtensa ESP32 lors de la première compilation.

---

## Prérequis développeur

### Tous systèmes

| Outil | Version recommandée |
|---|---|
| Git | 2.x ou plus récent |
| .NET SDK | 10.x |
| PlatformIO CLI | dernière version stable |
| Visual Studio Code | recommandé, mais optionnel pour compiler l’IDE |

Vérifier les outils :

```bash
git --version
dotnet --version
pio --version
code --version
```

`pio` et `code` ne sont pas nécessaires pour compiler l’application elle-même, mais seront nécessaires pour les fonctions Build, Flash, Monitor et ouverture de projets.

---

## Installation de PlatformIO CLI

PlatformIO peut être installé de plusieurs manières. Par exemple avec Python :

```bash
python -m pip install --user platformio
```

Sous Linux, `pio` est souvent installé dans :

```text
~/.local/bin/pio
```

Il faut donc vérifier que `~/.local/bin` est présent dans la variable `PATH`.

Exemple :

```bash
export PATH="$HOME/.local/bin:$PATH"
pio --version
```

---

## Prérequis Linux supplémentaires

Pour accéder à une Gamebuino AKA connectée en USB, l’utilisateur doit disposer des droits sur le port série.

Les ports sont généralement nommés :

```text
/dev/ttyACM0
/dev/ttyUSB0
```

Sur Ubuntu/Debian, l’utilisateur doit souvent appartenir au groupe `dialout` :

```bash
sudo usermod -aG dialout "$USER"
```

Ferme ensuite la session Linux et reconnecte-toi.

> L’application ne doit pas être lancée avec `sudo`.

---

## Compilation depuis les sources

Cloner le dépôt :

```bash
git clone https://github.com/Jicehel-Aka/AKA-IDE-Linux-.git
cd AKA-IDE-Linux-
git checkout avalonia
```

Restaurer les dépendances :

```bash
dotnet restore GamebuinoAKA.sln
```

Compiler :

```bash
dotnet build GamebuinoAKA.sln -c Release
```

Exécuter les tests :

```bash
dotnet test GamebuinoAKA.sln -c Release
```

Lancer l’application en développement :

```bash
dotnet run --project src/GamebuinoAKA.App/GamebuinoAKA.App.csproj
```

---

## Publication Linux

Créer un binaire Linux x64 autonome :

```bash
dotnet publish src/GamebuinoAKA.App/GamebuinoAKA.App.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o artifacts/linux-x64
```

Le résultat est créé dans :

```text
artifacts/linux-x64/
```

Sous Linux, rendre l’exécutable utilisable si nécessaire :

```bash
chmod +x artifacts/linux-x64/GamebuinoAKA.App
```

Puis le lancer :

```bash
./artifacts/linux-x64/GamebuinoAKA.App
```

---

## GitHub Actions

Chaque `push` sur `main` ou `avalonia` déclenche une compilation Ubuntu via GitHub Actions.

Le workflow effectue :

1. installation de .NET 10 ;
2. restauration NuGet ;
3. vérification qu’aucune dépendance WPF n’est introduite dans `Core` ;
4. compilation de la solution ;
5. exécution des tests unitaires ;
6. publication d’un binaire `linux-x64` ;
7. dépôt du binaire dans les artifacts GitHub Actions.

Les builds sont visibles ici :

<https://github.com/Jicehel-Aka/AKA-IDE-Linux-/actions>

---

## Couleurs Gamebuino AKA

La Gamebuino AKA utilise le format documenté dans le projet comme **BGR565 AKA**.

| Couleur | Valeur BGR565 AKA |
|---|---:|
| Noir | `0x0000` |
| Rouge | `0x001F` |
| Vert | `0x07E0` |
| Bleu | `0xF800` |
| Blanc | `0xFFFF` |
| Magenta transparent | `0xF81F` |

La transparence historique utilise la clé magenta :

```cpp
0xF81F
```

Les conversions BGR565 font l’objet de tests unitaires afin d’éviter une inversion rouge/bleu.

---

## Ancienne version WPF

Le code Windows/WPF historique est conservé dans :

```text
legacy-wpf/
```

Il sert de référence fonctionnelle pendant la migration.

Il ne fait pas partie de la nouvelle solution et ne doit pas être compilé par GitHub Actions dans le cadre du portage Avalonia.

---

## Licence

MIT — voir [LICENSE](LICENSE).

---

Fait avec ❤️ pour la communauté Gamebuino AKA.
