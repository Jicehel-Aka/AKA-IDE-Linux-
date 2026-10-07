# Architecture logicielle — Gamebuino AKA IDE

## 1. Séparation stricte Core / App / Tests

- **GamebuinoAKA.Core** : Ne contient aucune dépendance vers WPF, Avalonia, ni System.Drawing.
  - `Models/` : Données pures sérialisables.
  - `Platform/` : Abstractions (`IPlatformPaths`, `IProcessRunner`, `IToolLocator`, `IApplicationLauncher`).
  - `Services/` : Logique de build, génération C++, SkiaSharp, snippets.
- **GamebuinoAKA.App** : Hôte de présentation Avalonia 11 (MVVM CommunityToolkit).
  - `Controls/` : `PixelCanvas` et `GridCanvas` avec rendu direct Skia/Avalonia.
  - `Platform/` : `LinuxPlatformPaths` (spécification XDG) et `WindowsPlatformPaths`.
  - `ViewModels/` & `Views/` : Couplage par convention via `ViewLocator`.
