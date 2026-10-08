using System;
using System.IO;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly ISettingsService _settings;

        public TemplateService(ISettingsService settings) => _settings = settings;

        public async Task CreateProjectAsync(string projectName, string template, string destinationFolder, BuildSystem buildSystem, bool withAudio = false)
        {
            var projectDir = Path.Combine(destinationFolder, projectName);
            Directory.CreateDirectory(projectDir);

            if (buildSystem == BuildSystem.EspIdf)
            {
                var mainDir = Path.Combine(projectDir, "main");
                Directory.CreateDirectory(mainDir);
                await File.WriteAllTextAsync(Path.Combine(projectDir, "CMakeLists.txt"), $"cmake_minimum_required(VERSION 3.16)\ninclude($ENV{{IDF_PATH}}/tools/cmake/project.cmake)\nproject({projectName})\n");
                await File.WriteAllTextAsync(Path.Combine(mainDir, "CMakeLists.txt"), "idf_component_register(SRCS app_main.cpp INCLUDE_DIRS . REQUIRES gamebuino)\n");
                await File.WriteAllTextAsync(Path.Combine(mainDir, "app_main.cpp"), $"#include \"freertos/FreeRTOS.h\"\n#include \"freertos/task.h\"\n#include \"gamebuino.h\"\ngb_core g_core;\ngb_graphics gfx;\nextern \"C\" void app_main() {{ g_core.init(); while(true) {{ g_core.pool(); gfx.clear(0); gfx.update(); vTaskDelay(pdMS_TO_TICKS(16)); }} }}\n");
            }
            else
            {
                var srcDir = Path.Combine(projectDir, "src");
                Directory.CreateDirectory(srcDir);
                await File.WriteAllTextAsync(Path.Combine(projectDir, "platformio.ini"), "[env:gamebuino_aka]
platform = espressif32
board = esp32-s3-devkitc-1
framework = arduino
");
                await File.WriteAllTextAsync(Path.Combine(srcDir, "main.cpp"), "#include <Gamebuino-AKA.h>
Gamebuino gb;
void setup() { gb.begin(); }
void loop() { gb.waitForUpdate(); gb.display.clear(); }
");
            }
        }
    }
}
