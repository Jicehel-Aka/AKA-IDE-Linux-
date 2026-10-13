# sims/ — simulateurs PC embarqués

L'IDE cherche ici les simulateurs des projets script (bouton « ▶ Tester (PC) »
de l'écran Scripts) :

```
sims/akalove-play[.exe]   # simulateur Love2D  (dépôt akalove)
sims/aka-py-play[.exe]     # simulateur MicroPython (dépôt MycroPython)
sims/SDL2.dll             # (Windows) requise par les deux simulateurs
sims/akatransfer.py       # transfert série vers la console réelle
```

## D'où viennent-ils ?

À la **release**, la CI (`.github/workflows/ci.yml`, étapes « Bundle PC
simulators ») récupère automatiquement les binaires depuis les dernières
releases des dépôts **MycroPython** (`aka-py-play`) et **akalove**
(`akalove-play`), plus `akatransfer.py`, et les place ici dans le paquet publié.
Rien à faire à la main pour une release.

Pour un **build local de dev**, ce dossier est vide (seul ce README est
versionné). Deux options :
- déposer les binaires compilés ici (mêmes noms que ci-dessus), ou
- renseigner leur chemin dans **Paramètres → Simulateurs PC** (prioritaire sur
  l'embarqué).

On récupère les binaires ainsi (exemple Linux) :
```bash
gh release download --repo Jicehel-Aka/MycroPython --pattern 'micropython-aka-sim-linux-x64-*.zip'
gh release download --repo Jicehel-Aka/akalove       --pattern 'aka-love-sim-linux-x64-*.zip'
# puis copier aka-py-play et akalove-play dans ce dossier.
```

## Contrat de ligne de commande

```
akalove-play  "<dossier_du_jeu>"    # dossier contenant main.lua/conf.lua
aka-py-play    "<script.py>"         # script d'entrée (ex: main.py)
```
