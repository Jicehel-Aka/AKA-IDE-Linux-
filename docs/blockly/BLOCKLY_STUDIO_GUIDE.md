# 🧩 Guide Complet du Studio Blockly Gamebuino AKA

Bienvenue dans la documentation officielle du **Studio Blockly pour Gamebuino AKA** (console portable rétro propulsée par l'ESP32-S3).

---

## 🚀 1. Architecture Générale
Le Studio Blockly permet de programmer la console Gamebuino AKA par assemblage visuel de blocs colorés, avec traduction instantanée en :
- **MicroPython** (`main.py`) : exécutable directement sur le firmware MicroPython embarqué avec la bibliothèque `gamebuino`.
- **C++ Arduino / PlatformIO** : code natif ultra-optimisé 60 FPS avec la bibliothèque officielle `Gamebuino-AKA.h`.

---

## 🎨 2. Catégories de Blocs Disponibles
1. **🚀 Démarrage & Boucle 60 FPS** (`gb_program_start`, `gb_game_loop`, `gb_fps`) : cadencement précis à 60 Hz pour une fluidité sans faille.
2. **🖥️ Graphismes & Écran 320×240** (`gb_clear_screen`, `gb_draw_rect`, `gb_draw_circle`, `gb_draw_line`, `gb_print_text`, palette de couleurs indexées).
3. **🎮 Manette & Contrôles** (`gb_button_pressed`, `gb_button_released`, flèches D-Pad, boutons A, B, Menu, Home).
4. **🔊 Synthétiseur & Bruits SFX** (`gb_play_tone`, fréquences Do-Ré-Mi-Fa-Sol-La-Si, effets SFX prédéfinis).
5. **✨ 4 LEDs RGB NeoPixels** (`gb_set_neopixel`, animations chenillard, battement de cœur, alarmes).
6. **📳 Moteur Haptique / Vibreur** (`gb_vibrate`) : retour tactile immersif lors des tirs, sauts ou alertes.
7. **👾 Éditeur de Sprites 16×16** : matrice interactive de pixels avec export bitmap hexadecimal automatique.

---

## 🎓 3. Les 9 Tutoriels Guidés Multilingues
Le Studio intègre 9 parcours pédagogiques progressifs traduits en **6 langues (FR, EN, DE, IT, PT, ES)** :
1. 🏓 **Pong Rétro Complet** (5 étapes)
2. 🚀 **Shoot'em Up Spatial** (6 étapes)
3. 🏃 **Jeu de Plateforme avec Gravité** (5 étapes)
4. 🟡 **Pac-Maze Labyrinthe** (5 étapes)
5. 🧮 **Calculatrice Rétro D-Pad** (4 étapes)
6. 🐾 **Animal Virtuel / Tamagotchi** (4 étapes)
7. 🎹 **Synthétiseur Chiptune & Boîte à Rythmes** (4 étapes)
8. ⏱️ **Chronomètre & Minuteur Pomodoro** (4 étapes)
9. ⏳ **Minuteur Cuisson des Œufs & Jeux de Société** (4 étapes)
