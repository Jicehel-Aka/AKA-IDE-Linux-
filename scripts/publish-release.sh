#!/bin/bash
# 🎮 Gamebuino AKA IDE — Script interactif de publication de Release
set -e

echo "=========================================================="
echo "🎮 Gamebuino AKA IDE — Publication d'une Release GitHub"
echo "=========================================================="

# Récupération du dernier tag existant
LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "aucun")
echo "Dernière version publiée : $LAST_TAG"
echo ""

# Demande interactive du numéro de version
read -p "Entrez le nouveau numéro de version à publier (ex: v1.0.0) : " VERSION

if [ -z "$VERSION" ]; then
  echo "❌ Erreur : Le numéro de version ne peut pas être vide."
  exit 1
fi

# Préfixer avec 'v' si omis
if [[ ! "$VERSION" =~ ^v ]]; then
  VERSION="v$VERSION"
fi

echo ""
echo "Version choisie : $VERSION"
read -p "Confirmer la publication de la release $VERSION sur GitHub ? (o/n) : " CONFIRM

if [[ "$CONFIRM" =~ ^[oOyY] ]]; then
  echo "📌 Création du tag Git local '$VERSION'..."
  git tag -a "$VERSION" -m "Gamebuino AKA IDE Release $VERSION"

  echo "🚀 Envoi du tag sur GitHub..."
  git push origin "$VERSION"

  echo ""
  echo "✅ Tag $VERSION poussé avec succès !"
  echo "👉 GitHub Actions démarre la compilation (Linux + Windows)."
  echo "👉 La Release officielle sera disponible dans quelques minutes sur votre dépôt GitHub !"
else
  echo "Annulation."
  exit 0
fi
