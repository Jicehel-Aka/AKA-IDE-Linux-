# Gamebuino AKA IDE — Avalonia Edition

Portage multiplateforme de Gamebuino AKA IDE.

## Stack technique

- C#
- .NET 10 (`net10.0`)
- Avalonia UI
- CommunityToolkit.Mvvm
- Bibliothèque image : à sélectionner (ImageSharp soumis à licence depuis v4)
- PlatformIO pour Build / Flash / Monitor

## Architecture

```text
src/
├── GamebuinoAKA.Core/       # Métier indépendant de toute UI
└── GamebuinoAKA.App/        # Interface graphique Avalonia

tests/
└── GamebuinoAKA.Core.Tests/

legacy-wpf/ # Application WPF historique conservée comme référence 
```
## Branches
main : état historique WPF ;
v1-wpf-final : tag de référence WPF ;
avalonia : nouveau portage Avalonia/Linux. 

## Commandes
dotnet restore
dotnet build -c Release
dotnet test -c Release EOF

cat >> .gitignore <<'EOF'

## Build .NET

bin/
obj/
artifacts/

##Visual Studio / JetBrains
.vs/
*.user
*.suo
