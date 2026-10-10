# sims/ — simulateurs PC embarqués

L'IDE cherche ici les simulateurs des projets script (bouton « ▶ Tester (PC) »
de l'écran Scripts). Dépose les binaires compilés à ces emplacements :

```
sims/aka-love-sim[.exe]   # simulateur Love2D (dépôt akalove, cible SDL2 aka-love-sim)
sims/aka-py-sim[.exe]     # simulateur MicroPython (à venir)
sims/akatransfer.py       # transfert série vers la console réelle
```

Un chemin configuré dans Paramètres → Simulateurs PC a toujours la priorité.

Contrat de ligne de commande :
```
aka-love-sim  "<dossier_du_jeu>"    # dossier contenant main.lua/conf.lua
aka-py-sim    "<script.py>"
```
