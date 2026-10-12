# Améliorations des Éditeurs Lua / Love2D et MicroPython (PC vs AKA)

Ce document formalise les axes d'amélioration prioritaires pour intégrer et perfectionner les environnements **Lua (Love2D)** et **MicroPython** dans l'écosystème **Gamebuino AKA**.

---

## 🎮 1. Éditeur Lua & Love2D

### Sur le PC (IDE Avalonia) :
1. **Hot-Reloading à chaud** :
   - Observer les modifications de fichiers avec un `FileSystemWatcher` C#.
   - Recharger les modules Lua modifiés sans relancer le processus et sans perdre les variables d'état du jeu.
2. **Linter Lua strict (Luacheck / Selene)** :
   - Surligner immédiatement les variables globales non déclarées (cause n°1 des bugs en Lua).
3. **Simulateur d'écran Gamebuino AKA** :
   - Émulation de la résolution native 320x240 (avec zoom pixel-perfect Nearest-Neighbor) et mode rétro basse résolution optionnel (160x120), simulation des formats de pixels RGB565 / BGR565.
4. **Export 1-Clic** :
   - Empaquetage automatique au format standard `.love` (archive zip sans compression).
   - Fusion avec le binaire exécutable pour générer un fichier autonome Linux (AppImage/tar.gz) ou Windows (.exe).

### Sur la Console AKA :
1. **Memory Guard & Garbage Collection cadencé** :
   - Déclencher `collectgarbage("step", 100)` à chaque frame pour éviter un blocage brutal du ramasse-miettes en pleine partie.
2. **Watchdog Anti-Boucle Infinie** :
   - Hook de débogage `lua_sethook` pour interrompre un script bloqué.
3. **REPL Série USB CDC** :
   - Permettre l'exécution de commandes Lua interactives envoyées depuis le PC.

---

## 🐍 2. Éditeur MicroPython

### Sur le PC (IDE Avalonia) :
1. **Précompilation Bytecode mpy-cross** :
   - Compiler les fichiers `.py` en bytecode `.mpy` avant envoi.
   - **Gains majeurs** : ~60% d'économie de mémoire RAM sur la console et démarrage instantané !
2. **Gestionnaire de fichiers USB (mpremote)** :
   - Téléversement direct dans `/flash` ou `/sd` par glisser-déposer sans sudo.
3. **Stubs de typage gamebuino.pyi** :
   - Autocomplétion riche des fonctions matérielles (`gb.display`, `gb.buttons`, `gb.sound`).
4. **Moniteur de mémoire Heap** :
   - Affichage en temps réel de `gc.mem_alloc()` et `gc.mem_free()`.

### Sur la Console AKA :
1. **Mode Secours "Safe Boot"** :
   - Maintenir le bouton B au démarrage pour ignorer `main.py` en cas de crash critique au démarrage.
2. **Frozen Modules dans la ROM** :
   - Inclure les drivers matériels dans le firmware Flash sans consommer de mémoire RAM.
3. **Patterns Zéro Allocation** :
   - Bannir les allocations d'objets dans `update()` pour garantir 60 FPS constants.
