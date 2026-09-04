# GamebuinoAKA.Core

Cœur métier multiplateforme de Gamebuino AKA IDE.

## Règles impératives

Le projet Core ne doit dépendre ni de WPF ni d'Avalonia.

Il ne doit jamais référencer :

- `System.Windows`
- `PresentationFramework`
- `PresentationCore`
- `WindowsBase`
- `System.Drawing.Common`
- `explorer.exe`
- `Code.exe`
- `pio.exe`
- `%APPDATA%`

Le Core accueillera progressivement les modèles, services, ViewModels
indépendants de l'UI, conversions BGR565, sprites, tilemaps et logique PlatformIO. La bibliothèque image sera choisie avant la migration de AssetService.
