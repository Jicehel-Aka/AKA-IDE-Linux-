using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GamebuinoAKA.Core.Models;
using Newtonsoft.Json;

namespace GamebuinoAKA.Core.Services
{
    public class CodeSnippetService : ICodeSnippetService
    {
        private const string BankFileName = ".aka-snippets.json";
        private readonly ISettingsService _settings;

        public CodeSnippetService(ISettingsService settings)
        {
            _settings = settings;
        }

        private string BankFilePath
        {
            get
            {
                var folder = _settings.Settings.WorkspaceFolder;
                if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                    folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "GamebuinoAKA");
                return Path.Combine(folder, BankFileName);
            }
        }

        public SnippetBank LoadUserBank()
        {
            try
            {
                if (File.Exists(BankFilePath))
                    return JsonConvert.DeserializeObject<SnippetBank>(File.ReadAllText(BankFilePath)) ?? new SnippetBank();
            }
            catch { }
            return new SnippetBank();
        }

        public void SaveUserBank(SnippetBank bank)
        {
            try { File.WriteAllText(BankFilePath, JsonConvert.SerializeObject(bank, Formatting.Indented)); }
            catch { }
        }

        public List<CodeSnippet> GetAll(BuildSystem? forBuild = null)
        {
            var all = new List<CodeSnippet>(BuiltinSnippets());
            all.AddRange(LoadUserBank().UserSnippets);

            if (forBuild == BuildSystem.PlatformIO)
                return all.Where(s => s.ForPlatformIO).ToList();
            if (forBuild == BuildSystem.EspIdf)
                return all.Where(s => s.ForEspIdf).ToList();
            return all;
        }

        public Dictionary<string, string> GenerateFiles(IEnumerable<CodeSnippet> selected, BuildSystem buildSystem, string projectName)
        {
            var snippets = ResolveDependencies(selected.ToList(), buildSystem);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (buildSystem == BuildSystem.PlatformIO)
                GeneratePlatformIO(snippets, projectName, result);
            else
                GenerateEspIdf(snippets, projectName, result);

            return result;
        }

        private static void GeneratePlatformIO(List<CodeSnippet> snippets, string projectName, Dictionary<string, string> result)
        {
            var forGameCpp   = snippets.Where(s => s.TargetFile == SnippetTargetFile.GameCpp).ToList();
            var forGameH     = snippets.Where(s => s.TargetFile == SnippetTargetFile.GameH).ToList();
            var forMain      = snippets.Where(s => s.TargetFile == SnippetTargetFile.MainCpp).ToList();
            var forNewH      = snippets.Where(s => s.TargetFile == SnippetTargetFile.NewHeader).ToList();
            var forNewCpp    = snippets.Where(s => s.TargetFile == SnippetTargetFile.NewCpp).ToList();

            if (forGameCpp.Count > 0 || forGameH.Count > 0)
            {
                result["src/game.h"]   = BuildGameH(forGameH, projectName);
                result["src/game.cpp"] = BuildGameCpp(forGameCpp, projectName);
                result["src/main.cpp"] = BuildMainCppWithGame(forMain, projectName);
            }
            else if (forMain.Count > 0)
            {
                result["src/main.cpp"] = BuildMainCppInline(forMain, projectName);
            }

            foreach (var s in forNewH) result[$"src/{Sanitize(s.Name)}.h"] = s.Code;
            foreach (var s in forNewCpp) result[$"src/{Sanitize(s.Name)}.cpp"] = s.Code;
        }

        private static void GenerateEspIdf(List<CodeSnippet> snippets, string projectName, Dictionary<string, string> result)
        {
            var forMain  = snippets.Where(s => s.TargetFile == SnippetTargetFile.MainCpp).ToList();
            var forNewH  = snippets.Where(s => s.TargetFile == SnippetTargetFile.NewHeader).ToList();
            var forNewCpp = snippets.Where(s => s.TargetFile == SnippetTargetFile.NewCpp).ToList();

            if (forMain.Count > 0) result["main/app_main.cpp"] = BuildAppMain(forMain, projectName);
            foreach (var s in forNewH) result[$"main/{Sanitize(s.Name)}.h"] = s.Code;
            foreach (var s in forNewCpp) result[$"main/{Sanitize(s.Name)}.cpp"] = s.Code;
        }

        private static string BuildGameH(List<CodeSnippet> snippets, string project)
        {
            var decls = Collect(snippets, "//@@DECLARATIONS@@");
            return $@"#pragma once
// {project} — game.h
// Généré par Gamebuino AKA IDE (ESP32-S3 320x240)
#include <Gamebuino-AKA.h>

void gameUpdate(Gamebuino& gb);
void gameRender(Gamebuino& gb);

{(decls.Length > 0 ? "// ── Déclarations des modules sélectionnés ──────────────────────────────\n" + decls : "")}
";
        }

        private static string BuildGameCpp(List<CodeSnippet> snippets, string project)
        {
            var includes  = Collect(snippets, "//@@INCLUDES@@");
            var globals   = Collect(snippets, "//@@GLOBALS@@");
            var update    = Collect(snippets, "//@@UPDATE@@");
            var render    = Collect(snippets, "//@@RENDER@@");
            var functions = Collect(snippets, "//@@FUNCTIONS@@");

            return $@"#include ""game.h""
{(includes.Length > 0 ? includes + "\n" : "")}
static int playerX = 160;
static int playerY = 120;
static const int PLAYER_SPEED = 2;
{(globals.Length > 0 ? "\n" + globals : "")}
void gameUpdate(Gamebuino& gb) {{
    if (gb.buttons.repeat(BUTTON_LEFT,  1)) playerX -= PLAYER_SPEED;
    if (gb.buttons.repeat(BUTTON_RIGHT, 1)) playerX += PLAYER_SPEED;
    if (gb.buttons.repeat(BUTTON_UP,    1)) playerY -= PLAYER_SPEED;
    if (gb.buttons.repeat(BUTTON_DOWN,  1)) playerY += PLAYER_SPEED;
    playerX = max(0, min(315, playerX));
    playerY = max(0, min(235, playerY));
{(update.Length > 0 ? "\n" + Indent(update, 4) : "")}
}}

void gameRender(Gamebuino& gb) {{
    gb.display.setColor(BLACK);
    gb.display.fill();
    gb.display.setColor(0x7C5C);
    gb.display.fillRect(playerX, playerY, 5, 5);
    gb.display.setColor(WHITE);
    gb.display.setCursor(4, 4);
    gb.display.print(""{project}"");
{(render.Length > 0 ? "\n" + Indent(render, 4) : "")}
}}
{(functions.Length > 0 ? "\n// ── Fonctions helper ─────────────────────────────────────────────────\n" + functions : "")}
";
        }

        private static string BuildMainCppWithGame(List<CodeSnippet> snippets, string project)
        {
            return $@"#include <Gamebuino-AKA.h>
#include ""game.h""
Gamebuino gb;
void setup() {{ gb.begin(); }}
void loop() {{
    gb.waitForUpdate();
    gb.display.clear();
    gameUpdate(gb);
    gameRender(gb);
}}
";
        }

        private static string BuildMainCppInline(List<CodeSnippet> snippets, string project)
        {
            var includes  = Collect(snippets, "//@@INCLUDES@@");
            var globals   = Collect(snippets, "//@@GLOBALS@@");
            var update    = Collect(snippets, "//@@UPDATE@@");
            var render    = Collect(snippets, "//@@RENDER@@");
            var functions = Collect(snippets, "//@@FUNCTIONS@@");

            return $@"#include <Gamebuino-AKA.h>
{(includes.Length > 0 ? includes + "\n" : "")}
Gamebuino gb;
{(globals.Length > 0 ? "\n" + globals : "")}
void setup() {{ gb.begin(); }}
void loop() {{
    gb.waitForUpdate();
    gb.display.clear();
{(update.Length > 0 ? Indent(update, 4) + "\n" : "")}
{(render.Length > 0 ? Indent(render, 4) + "\n" : "")}
}}
{(functions.Length > 0 ? "\n" + functions : "")}
";
        }

        private static string BuildAppMain(List<CodeSnippet> snippets, string project)
        {
            var includes  = Collect(snippets, "//@@INCLUDES@@");
            var globals   = Collect(snippets, "//@@GLOBALS@@");
            var setup     = Collect(snippets, "//@@SETUP@@");
            var update    = Collect(snippets, "//@@UPDATE@@");
            var render    = Collect(snippets, "//@@RENDER@@");
            var functions = Collect(snippets, "//@@FUNCTIONS@@");

            return $@"/*
 * app_main.cpp — {project} (Gamebuino AKA, ESP-IDF)
 */
#include ""freertos/FreeRTOS.h""
#include ""freertos/task.h""
#include ""gamebuino.h""
{(includes.Length > 0 ? includes + "\n" : "")}
gb_core     g_core;
gb_graphics gfx;

static int playerX = 160;
static int playerY = 120;
static const int PLAYER_SPEED = 2;
{(globals.Length > 0 ? "\n" + globals : "")}
{(functions.Length > 0 ? functions + "\n" : "")}
extern ""C"" void app_main(void)
{{
    g_core.init();
{(setup.Length > 0 ? "\n" + Indent(setup, 4) + "\n" : "")}
    while (true) {{
        g_core.pool();
        gfx.clear(gfx.makeColor(20, 16, 40));
{(update.Length > 0 ? Indent(update, 8) + "\n" : "")}
{(render.Length > 0 ? Indent(render, 8) + "\n" : "")}
        gfx.update();
        vTaskDelay(pdMS_TO_TICKS(16));
    }}
}}
";
        }

        private static string Collect(IEnumerable<CodeSnippet> snippets, string marker)
        {
            var lines = new System.Text.StringBuilder();
            foreach (var s in snippets)
            {
                var section = ExtractSection(s.Code, marker);
                if (section.Length > 0)
                {
                    lines.AppendLine($"    // --- {s.Name} ---");
                    lines.AppendLine(section);
                }
            }
            return lines.ToString().TrimEnd();
        }

        private static string ExtractSection(string code, string marker)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;
            var start = code.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return string.Empty;
            start = code.IndexOf('\n', start);
            if (start < 0) return string.Empty;
            start++;
            var nextMarker = code.IndexOf("//@@", start, StringComparison.Ordinal);
            var section = nextMarker < 0 ? code[start..] : code[start..nextMarker];
            return section.TrimEnd();
        }

        private static string Indent(string code, int spaces)
        {
            var pad = new string(' ', spaces);
            return string.Join('\n', code.Split('\n').Select(l => string.IsNullOrWhiteSpace(l) ? l : pad + l));
        }

        private static string Sanitize(string name) =>
            System.Text.RegularExpressions.Regex.Replace(string.IsNullOrEmpty(name) ? "module" : name, @"[^a-zA-Z0-9_]", "_");

        private List<CodeSnippet> ResolveDependencies(List<CodeSnippet> selected, BuildSystem build)
        {
            var all   = GetAll(build).ToDictionary(s => s.Id);
            var ids   = new HashSet<string>(selected.Select(s => s.Id));
            var queue = new Queue<CodeSnippet>(selected);

            while (queue.Count > 0)
            {
                var s = queue.Dequeue();
                foreach (var dep in s.RequiresSnippetIds)
                {
                    if (!ids.Contains(dep) && all.TryGetValue(dep, out var depSnippet))
                    {
                        ids.Add(dep);
                        queue.Enqueue(depSnippet);
                        selected.Add(depSnippet);
                    }
                }
            }
            return selected;
        }

        private static List<CodeSnippet> BuiltinSnippets() => new()
        {
            // === ENTREES (PlatformIO & ESP-IDF) ===
            new CodeSnippet
            {
                Id = "input_buttons", Name = "Lecture des boutons (PlatformIO)", Summary = "Lit A, B, directions et Menu à chaque frame.",
                Category = "Entrées", ForPlatformIO = true, ForEspIdf = false, TargetFile = SnippetTargetFile.GameCpp,
                Code = @"//@@UPDATE@@
    if (gb.buttons.repeat(BUTTON_LEFT,  1)) playerX -= PLAYER_SPEED;
    if (gb.buttons.repeat(BUTTON_RIGHT, 1)) playerX += PLAYER_SPEED;
    if (gb.buttons.repeat(BUTTON_UP,    1)) playerY -= PLAYER_SPEED;
    if (gb.buttons.repeat(BUTTON_DOWN,  1)) playerY += PLAYER_SPEED;
"
            },
            new CodeSnippet
            {
                Id = "esp_input", Name = "Entrées coquille (ESP-IDF)", Summary = "Boutons et joystick analogique.",
                Category = "Entrées", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@UPDATE@@
        uint16_t held = g_core.buttons.state() | g_core.joystick.state();
        if (held & gb_buttons::KEY_LEFT)  playerX -= PLAYER_SPEED;
        if (held & gb_buttons::KEY_RIGHT) playerX += PLAYER_SPEED;
        if (held & gb_buttons::KEY_UP)    playerY -= PLAYER_SPEED;
        if (held & gb_buttons::KEY_DOWN)  playerY += PLAYER_SPEED;
"
            },
            new CodeSnippet
            {
                Id = "esp_input_just_pressed", Name = "Bouton simple pression (ESP-IDF)", Summary = "Détecte l'appui unique sans répétition.",
                Category = "Entrées", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@UPDATE@@
        if (g_core.buttons.justPressed(gb_buttons::KEY_A)) {
            // Action sur appui bouton A
        }
"
            },
            new CodeSnippet
            {
                Id = "esp_input_menu", Name = "Touche Menu / Pause (ESP-IDF)", Summary = "Interception de la touche Home / Menu.",
                Category = "Entrées", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@UPDATE@@
        if (g_core.buttons.justPressed(gb_buttons::KEY_MENU)) {
            isPaused = !isPaused;
        }
"
            },

            // === GRAPHISMES & AFFICHAGE (ESP-IDF) ===
            new CodeSnippet
            {
                Id = "esp_gfx_shapes", Name = "Formes de base (ESP-IDF)", Summary = "Rectangle, contour, ligne, cercle avec gb_graphics.",
                Category = "Graphismes", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@RENDER@@
        gfx.setColor(gfx.makeColor(0, 120, 255));
        gfx.fillRect(playerX, playerY, 16, 16);
"
            },
            new CodeSnippet
            {
                Id = "gfx_fill_background", Name = "Fond coloré (PlatformIO)", Summary = "Efface l'écran avec une couleur de fond.",
                Category = "Graphismes", ForPlatformIO = true, ForEspIdf = false, TargetFile = SnippetTargetFile.GameCpp,
                Code = @"//@@RENDER@@
    gb.display.setColor(BLACK);
    gb.display.fill();
"
            },
            new CodeSnippet
            {
                Id = "esp_gfx_clear", Name = "Effacer l'écran (ESP-IDF)", Summary = "Nettoie le tampon graphique avec une couleur unie.",
                Category = "Graphismes", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@RENDER@@
        gfx.clear(gfx.makeColor(15, 15, 25));
"
            },
            new CodeSnippet
            {
                Id = "esp_gfx_circle", Name = "Cercle et Disque (ESP-IDF)", Summary = "Tracé de cercles pleins et contours.",
                Category = "Graphismes", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@RENDER@@
        gfx.setColor(gfx.makeColor(255, 200, 0));
        gfx.fillCircle(playerX + 8, playerY + 8, 8);
"
            },
            new CodeSnippet
            {
                Id = "esp_gfx_line", Name = "Tracé de ligne (ESP-IDF)", Summary = "Dessine une ligne droite entre deux coordonnées.",
                Category = "Graphismes", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@RENDER@@
        gfx.setColor(gfx.makeColor(255, 255, 255));
        gfx.drawLine(0, 0, playerX, playerY);
"
            },
            new CodeSnippet
            {
                Id = "esp_gfx_text", Name = "Affichage texte & score (ESP-IDF)", Summary = "Écriture de chaîne et variables numériques.",
                Category = "Graphismes", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@RENDER@@
        gfx.setColor(gfx.makeColor(255, 255, 255));
        gfx.setCursor(4, 4);
        gfx.printf(""Score: %d"", score);
"
            },

            // === PHYSIQUE & MOUVEMENTS (ESP-IDF) ===
            new CodeSnippet
            {
                Id = "esp_phys_gravity", Name = "Gravité et Saut (ESP-IDF)", Summary = "Gestion de l'accélération verticale et du sol.",
                Category = "Physique", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@GLOBALS@@
static float velY = 0;
static const float GRAVITY = 0.4f;
static bool onGround = false;
//@@UPDATE@@
        velY += GRAVITY;
        playerY += (int)velY;
        if (playerY >= 100) { playerY = 100; velY = 0; onGround = true; }
        if (onGround && (held & gb_buttons::KEY_A)) { velY = -6.0f; onGround = false; }
"
            },
            new CodeSnippet
            {
                Id = "esp_phys_bounce", Name = "Rebond sur les bords (ESP-IDF)", Summary = "Inversion de vecteur vitesse aux limites de l'écran.",
                Category = "Physique", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@GLOBALS@@
static int speedX = 2, speedY = 2;
//@@UPDATE@@
        playerX += speedX;
        playerY += speedY;
        if (playerX <= 0 || playerX >= 150) speedX = -speedX;
        if (playerY <= 0 || playerY >= 120) speedY = -speedY;
"
            },
            new CodeSnippet
            {
                Id = "esp_phys_aabb", Name = "Collision Boîte AABB (ESP-IDF)", Summary = "Détection de chevauchement entre deux rectangles.",
                Category = "Physique", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@FUNCTIONS@@
static bool checkCollision(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2) {
    return (x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2);
}
"
            },

            // === AUDIO & RETOURS (ESP-IDF) ===
            new CodeSnippet
            {
                Id = "esp_audio_beep", Name = "Bip sonore chiptune (ESP-IDF)", Summary = "Émission d'un son bref via le buzzer.",
                Category = "Audio", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@UPDATE@@
        if (g_core.buttons.justPressed(gb_buttons::KEY_B)) {
            g_core.sound.playTone(880, 50);
        }
"
            },
            new CodeSnippet
            {
                Id = "esp_audio_laser", Name = "Son de tir laser (ESP-IDF)", Summary = "Effet sonore à fréquence modulée descendante.",
                Category = "Audio", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@FUNCTIONS@@
static void playLaserSound() {
    for (int f = 1200; f > 300; f -= 100) {
        g_core.sound.playTone(f, 5);
    }
}
"
            },

            // === UTILITAIRES & LOGIQUE DE JEU (ESP-IDF) ===
            new CodeSnippet
            {
                Id = "esp_logic_timer", Name = "Chronomètre / Delta Time (ESP-IDF)", Summary = "Mesure du temps écoulé avec FreeRTOS tick.",
                Category = "Logique", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@GLOBALS@@
static uint32_t lastTick = 0;
//@@UPDATE@@
        uint32_t now = xTaskGetTickCount();
        uint32_t deltaMs = (now - lastTick) * portTICK_PERIOD_MS;
        lastTick = now;
"
            },
            new CodeSnippet
            {
                Id = "esp_logic_camera", Name = "Caméra avec centrage dynamique (ESP-IDF)", Summary = "Calcul de l'offset caméra suivant le joueur.",
                Category = "Logique", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@GLOBALS@@
static int cameraX = 0;
//@@UPDATE@@
        cameraX = playerX - (160 / 2);
        if (cameraX < 0) cameraX = 0;
"
            },
            new CodeSnippet
            {
                Id = "esp_logic_highscore", Name = "Persistance Score en NVS (ESP-IDF)", Summary = "Sauvegarde du meilleur score dans la Flash non-volatile.",
                Category = "Logique", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@INCLUDES@@
#include ""nvs_flash.h""
#include ""nvs.h""
//@@FUNCTIONS@@
static void saveHighScore(int val) {
    nvs_handle_t h;
    if (nvs_open(""storage"", NVS_READWRITE, &h) == ESP_OK) {
        nvs_set_i32(h, ""hiscore"", val);
        nvs_commit(h);
        nvs_close(h);
    }
}
"
            },
            new CodeSnippet
            {
                Id = "esp_logic_particle", Name = "Émetteur de particules (ESP-IDF)", Summary = "Système d'étincelles ou d'étoiles animées.",
                Category = "Graphismes", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@GLOBALS@@
struct Particle { float x, y, vx, vy; int life; };
static Particle particles[16];
//@@RENDER@@
        for (int i = 0; i < 16; i++) {
            if (particles[i].life > 0) {
                gfx.drawPixel((int)particles[i].x, (int)particles[i].y, gfx.makeColor(255, 255, 100));
            }
        }
"
            },
            new CodeSnippet
            {
                Id = "esp_logic_state_machine", Name = "Machine à États Titre/Jeu/GameOver (ESP-IDF)", Summary = "Gestion des états de l'application.",
                Category = "Logique", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@GLOBALS@@
enum GameState { STATE_TITLE, STATE_PLAY, STATE_GAMEOVER };
static GameState currentState = STATE_TITLE;
//@@UPDATE@@
        switch (currentState) {
            case STATE_TITLE:
                if (held & gb_buttons::KEY_A) currentState = STATE_PLAY;
                break;
            case STATE_PLAY:
                // Logique en jeu
                break;
            case STATE_GAMEOVER:
                if (held & gb_buttons::KEY_B) currentState = STATE_TITLE;
                break;
        }
"
            },
            new CodeSnippet
            {
                Id = "esp_system_battery", Name = "Indicateur de batterie (ESP-IDF)", Summary = "Lecture du niveau de charge et affichage.",
                Category = "Système", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@RENDER@@
        int batteryPct = g_core.battery.percent();
        gfx.setColor(batteryPct < 20 ? gfx.makeColor(255, 0, 0) : gfx.makeColor(0, 255, 0));
        gfx.drawRect(140, 4, 16, 8);
        gfx.fillRect(142, 6, (batteryPct * 12) / 100, 4);
"
            },
            new CodeSnippet
            {
                Id = "esp_system_leds", Name = "Contrôle des LEDs RVB (ESP-IDF)", Summary = "Allumage des effets lumineux de façade.",
                Category = "Système", ForPlatformIO = false, ForEspIdf = true, TargetFile = SnippetTargetFile.MainCpp,
                Code = @"//@@UPDATE@@
        if (held & gb_buttons::KEY_A) {
            g_core.leds.setColor(255, 0, 128); // Rose néon
        } else {
            g_core.leds.setColor(0, 0, 0);
        }
"
            }
        };
    }
}
