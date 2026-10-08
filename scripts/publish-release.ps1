# 🎮 Gamebuino AKA IDE — Script interactif Windows PowerShell de publication
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "🎮 Gamebuino AKA IDE — Publication d'une Release GitHub" -ForegroundColor Cyan
Write-Host "=========================================================="

$lastTag = git describe --tags --abbrev=0 2>$null
if (-not $lastTag) { $lastTag = "aucun" }
Write-Host "Dernière version publiée : $lastTag"
Write-Host ""

$version = Read-Host "Entrez le nouveau numéro de version à publier (ex: v1.0.0)"
if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Host "❌ Erreur : Le numéro de version ne peut pas être vide." -ForegroundColor Red
    exit 1
}
if (-not $version.StartsWith("v")) { $version = "v$version" }

$confirm = Read-Host "Confirmer la publication de la release $version sur GitHub ? (o/n)"
if ($confirm -match "^[oOyY]") {
    Write-Host "📌 Création du tag Git local '$version'..."
    git tag -a "$version" -m "Gamebuino AKA IDE Release $version"
    Write-Host "🚀 Envoi du tag sur GitHub..."
    git push origin "$version"
    Write-Host ""
    Write-Host "✅ Tag $version poussé avec succès !" -ForegroundColor Green
    Write-Host "👉 GitHub compile et publie la release avec les fichiers .tar.gz et .zip."
} else {
    Write-Host "Annulation."
}
