using System;
using System.IO;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Génère un projet AKA : PlatformIO (Arduino) ou ESP-IDF (coquille).
    /// La chaîne ESP-IDF écrit le squelette complet (CMake, sdkconfig, partitions,
    /// game_module.h), copie le composant « gamebuino » (chemin de référence configuré,
    /// sinon lib embarquée, sinon placeholder), pose le marqueur .aka-build, et — quand
    /// withAudio est vrai — ajoute un module audio minimal (mixeur + tâche FreeRTOS).
    /// </summary>
    public class TemplateService : ITemplateService
    {
        private readonly ISettingsService _settings;

        public TemplateService(ISettingsService settings) => _settings = settings;

        public async Task CreateProjectAsync(string projectName, string template,
            string destinationFolder, BuildSystem buildSystem, bool withAudio = false)
        {
            var projectDir = Path.Combine(destinationFolder, projectName);
            Directory.CreateDirectory(projectDir);

            if (buildSystem == BuildSystem.EspIdf)
                await CreateEspIdfProjectAsync(projectName, projectDir, withAudio);
            else
                await CreatePlatformIOProjectAsync(projectName, template, projectDir);
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  PlatformIO (Arduino)
        // ═══════════════════════════════════════════════════════════════════════
        private async Task CreatePlatformIOProjectAsync(string projectName, string template, string projectDir)
        {
            var srcDir = Path.Combine(projectDir, "src");
            Directory.CreateDirectory(srcDir);

            await Write(projectDir, "platformio.ini",
@"[env:gamebuino_aka]
platform = espressif32
board = esp32-s3-devkitc-1
framework = arduino
monitor_speed = 115200
");
            await Write(srcDir, "main.cpp", PioMainCpp(template));
            await Write(projectDir, GamebuinoProject.BuildMarkerFile, "platformio");
        }

        private static string PioMainCpp(string template) => template switch
        {
            "hello-world" =>
@"#include <Gamebuino-AKA.h>
Gamebuino gb;
void setup() { gb.begin(); }
void loop() {
    gb.waitForUpdate();
    gb.display.clear();
    gb.display.setCursor(16, 16);
    gb.display.print(""Hello Gamebuino AKA!"");
}
",
            "game-template" =>
@"#include <Gamebuino-AKA.h>
Gamebuino gb;
int playerX = 160, playerY = 120;
void setup() { gb.begin(); }
void loop() {
    gb.waitForUpdate();
    if (gb.buttons.repeat(BUTTON_LEFT,  1)) playerX -= 2;
    if (gb.buttons.repeat(BUTTON_RIGHT, 1)) playerX += 2;
    if (gb.buttons.repeat(BUTTON_UP,    1)) playerY -= 2;
    if (gb.buttons.repeat(BUTTON_DOWN,  1)) playerY += 2;
    gb.display.clear();
    gb.display.fillRect(playerX, playerY, 16, 16);
}
",
            _ =>
@"#include <Gamebuino-AKA.h>
Gamebuino gb;
void setup() { gb.begin(); }
void loop() {
    gb.waitForUpdate();
    gb.display.clear();
}
"
        };

        // ═══════════════════════════════════════════════════════════════════════
        //  ESP-IDF (coquille)
        // ═══════════════════════════════════════════════════════════════════════
        private async Task CreateEspIdfProjectAsync(string projectName, string projectDir, bool withAudio)
        {
            var mainDir = Path.Combine(projectDir, "main");
            var vscodeDir = Path.Combine(projectDir, ".vscode");
            var compGb = Path.Combine(projectDir, "components", "gamebuino");
            Directory.CreateDirectory(mainDir);
            Directory.CreateDirectory(vscodeDir);
            Directory.CreateDirectory(Path.Combine(projectDir, "components"));

            await Write(projectDir, "CMakeLists.txt", IdfRootCMake(projectName));
            await Write(projectDir, GamebuinoProject.BuildMarkerFile, "espidf");
            await Write(projectDir, "sdkconfig.defaults", IdfSdkconfigDefaults());
            await Write(projectDir, "partitions.csv", IdfPartitions());
            await Write(projectDir, "README.md", IdfProjectReadme(projectName));

            await Write(mainDir, "CMakeLists.txt", IdfMainCMake(withAudio));
            await Write(mainDir, "app_main.cpp", IdfAppMain(projectName, withAudio));
            await Write(mainDir, "game_module.h", IdfGameModuleH());
            if (withAudio)
            {
                await Write(mainDir, "audio.h", IdfAudioHeader());
                await Write(mainDir, "audio.cpp", IdfAudioSource());
            }

            await Write(vscodeDir, "settings.json", IdfVsCodeSettings());
            await Write(vscodeDir, "extensions.json", IdfExtensionsJson());

            // Composant gamebuino : priorité au dossier de référence configuré,
            // sinon la lib EMBARQUÉE dans l'IDE (Lib/gamebuino), sinon placeholder.
            var refPath = _settings.Settings.ReferenceGamebuinoComponentPath;
            string? source = (!string.IsNullOrWhiteSpace(refPath) && Directory.Exists(refPath))
                ? refPath
                : GetBundledComponentPath();

            if (source != null)
                await Task.Run(() => CopyDirectory(source, compGb));
            else
            {
                Directory.CreateDirectory(compGb);
                await Write(compGb, "AJOUTER_LA_LIB.md", IdfComponentPlaceholder());
            }
        }

        private static string IdfRootCMake(string projectName) =>
$@"cmake_minimum_required(VERSION 3.16)
include($ENV{{IDF_PATH}}/tools/cmake/project.cmake)
project({SanitizeCmake(projectName)})
";

        // Aligné sur gb_config.h : ESP32-S3, 16 Mo, PSRAM octale, partitions custom.
        private static string IdfSdkconfigDefaults() =>
@"CONFIG_IDF_TARGET=""esp32s3""
CONFIG_ESPTOOLPY_FLASHSIZE_16MB=y
CONFIG_ESPTOOLPY_FLASHSIZE=""16MB""
CONFIG_SPIRAM=y
CONFIG_SPIRAM_MODE_OCT=y
CONFIG_SPIRAM_SPEED_80M=y
CONFIG_PARTITION_TABLE_CUSTOM=y
CONFIG_PARTITION_TABLE_CUSTOM_FILENAME=""partitions.csv""
CONFIG_FREERTOS_HZ=1000
CONFIG_FATFS_LFN_HEAP=y
";

        private static string IdfPartitions() =>
@"# Name,   Type, SubType, Offset,   Size
nvs,      data, nvs,      ,         0x6000
phy_init, data, phy,      ,         0x1000
factory,  app,  factory,  ,         0x300000
storage,  data, fat,      ,         0x200000
";

        private static string IdfMainCMake(bool withAudio) =>
@"idf_component_register(
    SRCS
        app_main.cpp" + (withAudio ? @"
        audio.cpp" : "") + @"
    INCLUDE_DIRS
        .
    REQUIRES
        gamebuino nvs_flash app_update esp_partition esp_system fatfs
)
";

        private static string IdfAppMain(string projectName, bool withAudio) =>
@"/*
 * app_main.cpp — " + projectName + @" (Gamebuino AKA, ESP-IDF)
 * Généré par Gamebuino AKA IDE.
 */
#include ""freertos/FreeRTOS.h""
#include ""freertos/task.h""
#include ""gamebuino.h""
" + (withAudio ? "#include \"audio.h\"   // support audio (activé à la création)\n" : "") + @"
gb_core     g_core;
gb_graphics gfx;

static int playerX = 160;
static int playerY = 120;
static const int PLAYER_SPEED = 2;

extern ""C"" void app_main(void)
{
    g_core.init();
" + (withAudio ? "    audio_init();   // démarre la tâche audio (mixeur)\n" : "") + @"
    const uint16_t bg = gfx.makeColor(20, 16, 40);
    const uint16_t fg = gfx.makeColor(255, 255, 255);

    while (true) {
        g_core.pool();

        uint16_t held = g_core.buttons.state() | g_core.joystick.state();
        if (held & gb_buttons::KEY_LEFT)  playerX -= PLAYER_SPEED;
        if (held & gb_buttons::KEY_RIGHT) playerX += PLAYER_SPEED;
        if (held & gb_buttons::KEY_UP)    playerY -= PLAYER_SPEED;
        if (held & gb_buttons::KEY_DOWN)  playerY += PLAYER_SPEED;
        if (playerX < 0) playerX = 0; else if (playerX > 304) playerX = 304;
        if (playerY < 0) playerY = 0; else if (playerY > 224) playerY = 224;
" + (withAudio ? "        if (g_core.buttons.pressed(gb_buttons::KEY_A)) audio_beep(880, 80);\n" : "") + @"
        gfx.clear(bg);
        gfx.setColor(fg);
        gfx.move_cursor(16, 16);
        gfx.print_str(""" + projectName + @""");
        gfx.setColor(gfx.makeColor(0, 120, 255));
        gfx.fillRect(playerX, playerY, 16, 16);

        gfx.update();
        vTaskDelay(pdMS_TO_TICKS(16));
    }
}
";

        private static string IdfGameModuleH() =>
@"/*
 * game_module.h — interface « coquille » (inspirée de main/shell/game_module.h).
 * Dans une vraie coquille, un jeu n'implémente QUE ces fonctions ; la coquille
 * gère écran-titre, menu, i18n, options, son, sauvegarde et retour-loader.
 */
#pragma once
#include <cstdint>

struct GameModule {
    void (*init)();
    void (*update)();
    void (*render)();
    void (*on_pause)();
    void (*on_resume)();
    const char* (*title)();
};
";

        // ── Module audio minimal (fidèle à main/core/audio.cpp de mAKArena) ──────
        private static string IdfAudioHeader() =>
@"#pragma once
#include <cstdint>

// Support audio minimal. audio_init() est appelé une fois dans app_main.
void audio_init();
void audio_set_master_volume(uint8_t v);            // 0..255
void audio_sfx(float f0, float f1, float v0, float v1, uint16_t ms);
void audio_beep(float freq, uint16_t ms);
void audio_music_note(float freq, uint16_t ms);
";

        private static string IdfAudioSource() =>
@"/*
  audio.cpp — plomberie audio minimale (Gamebuino AKA, ESP-IDF).
  Le mixeur (gb_audio_player) doit être pompé en continu : une tâche FreeRTOS
  dédiée appelle g_player.pool(). Deux pistes : musique + effets.
*/
#include ""audio.h""
#include ""gb_audio_player.h""
#include ""gb_audio_track_tone.h""
#include ""gb_ll_audio.h""
#include ""freertos/FreeRTOS.h""
#include ""freertos/task.h""

static gb_audio_player     g_player;
static gb_audio_track_tone g_music;
static gb_audio_track_tone g_sfx;

static void audio_task(void*)
{
    while (true) {
        g_player.pool();
        vTaskDelay(pdMS_TO_TICKS(2));
    }
}

void audio_init()
{
    gb_ll_audio_set_volume(200);
    g_player.add_track(&g_music);
    g_player.add_track(&g_sfx);
    g_music.set_track_volume(1.0f);
    g_sfx.set_track_volume(1.0f);
    xTaskCreatePinnedToCore(audio_task, ""AudioTask"", 4096, nullptr, 5, nullptr, 0);
}

void audio_set_master_volume(uint8_t v) { gb_ll_audio_set_volume(v); }

void audio_sfx(float f0, float f1, float v0, float v1, uint16_t ms)
{
    g_sfx.play_tone(f0, f1, v0, v1, ms, gb_audio_track_tone::SQUARE);
}

void audio_beep(float freq, uint16_t ms)
{
    g_sfx.play_tone(freq, freq, 0.60f, 0.10f, ms, gb_audio_track_tone::SQUARE);
}

void audio_music_note(float freq, uint16_t ms)
{
    g_music.play_tone(freq, freq, 0.45f, 0.05f, ms, gb_audio_track_tone::TRIANGLE);
}
";

        private static string IdfVsCodeSettings() =>
@"{
    ""idf.adapterTargetName"": ""esp32s3"",
    ""C_Cpp.intelliSenseEngine"": ""default""
}
";

        private static string IdfExtensionsJson() =>
@"{
    ""recommendations"": [ ""espressif.esp-idf-extension"", ""ms-vscode.cpptools"" ]
}
";

        private static string IdfProjectReadme(string projectName) =>
$@"# {projectName} (Gamebuino AKA, ESP-IDF)

Projet généré par Gamebuino AKA IDE.

## Composant gamebuino
Le dossier `components/gamebuino` doit contenir la bibliothèque de jmp42.
S'il contient `AJOUTER_LA_LIB.md`, copiez-y la lib (ou configurez son chemin de
référence dans les Paramètres de l'IDE, puis recréez le projet).

## Build
```
idf.py build
idf.py -p <PORT> flash monitor
```
";

        private static string IdfComponentPlaceholder() =>
@"# Composant gamebuino manquant

Copiez ici la bibliothèque `components/gamebuino` (de jmp42), OU renseignez son
chemin de référence dans les Paramètres de l'IDE puis recréez le projet.

Sans elle, la compilation ESP-IDF échouera (gamebuino.h introuvable).
";

        // ═══════════════════════════════════════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════════════════════════════════════
        private static async Task Write(string dir, string file, string content)
            => await File.WriteAllTextAsync(Path.Combine(dir, file), content);

        /// <summary>Lib embarquée dans l'IDE : &lt;base&gt;/Lib/gamebuino (si présente).</summary>
        private static string? GetBundledComponentPath()
        {
            try
            {
                var p = Path.Combine(AppContext.BaseDirectory, "Lib", "gamebuino");
                return Directory.Exists(p) ? p : null;
            }
            catch { return null; }
        }

        private static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);
            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite: true);
            foreach (var dir in Directory.GetDirectories(source))
                CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
        }

        private static string SanitizeCmake(string name)
        {
            var chars = new System.Text.StringBuilder();
            foreach (var c in name)
                chars.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_');
            var s = chars.ToString();
            return string.IsNullOrEmpty(s) ? "aka_project" : s;
        }
    }
}
