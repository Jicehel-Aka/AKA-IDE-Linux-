# Intégration de l'IA & Partage de Code dans Gamebuino AKA IDE

Ce document décrit comment intégrer un assistant intelligent (basé sur Google Gemini ou toute API REST) dans Gamebuino AKA IDE pour assister les développeurs et leur permettre de partager leur code.

---

## 🎯 1. Cas d'Usage Principaux

1. **Mentor & Guide de Programmation Gamebuino AKA** :
   - Répond aux questions sur la bibliothèque C++ Gamebuino AKA (`gb.display`, `gb.buttons`, `gb.sound`, `gb.lights`, `gb.save`).
   - Adapte le code aux capacités matérielles de la console AKA : microcontrôleur ESP32-S3 Dual-Core Xtensa LX7 @ 240 MHz, 512 Ko SRAM + 8 Mo PSRAM, 60 FPS constants, écran LCD 320x240.
2. **Diagnostic & Résolution Automatique des Erreurs de Compilation** :
   - En cas d'échec de `pio run` ou `idf.py build`, l'IDE transmet les lignes d'erreurs du compilateur GCC à l'IA.
   - L'IA traduit le message technique en explications claires et propose le patch C++ corrigé.
3. **Recommandation & Injection de Procédures** :
   - Sélectionne intelligemment la bonne procédure parmi le catalogue des 53 snippets (physique, vélocité, rebond, timers, audio, etc.).
4. **Partage de Code Communautaire (GitHub Gist)** :
   - Permet à l'utilisateur de publier en 1 clic son sketch ou sa procédure sous forme de **GitHub Gist** public ou secret, avec description et balisage automatique.

---

## 🏗️ 2. Architecture C# (.NET 10 / Avalonia)

### A. Dans `GamebuinoAKA.Core/Services/IAssistantService.cs`
```csharp
using System.Threading.Tasks;

namespace GamebuinoAKA.Core.Services
{
    public interface IAssistantService
    {
        Task<string> AskAsync(string userQuestion, string? activeCode = null);
        Task<string> ExplainBuildErrorAsync(string errorMessage, string codeContext);
        Task<string> ShareGistAsync(string fileName, string content, string description);
    }
}
```

### B. Dans `GamebuinoAKA.Core/Services/GeminiAssistantService.cs`
Implémentation légère utilisant `HttpClient` et l'API Google Gemini :
```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GamebuinoAKA.Core.Services
{
    public class GeminiAssistantService : IAssistantService
    {
        private readonly HttpClient _http = new();
        private readonly ISettingsService _settings;

        public GeminiAssistantService(ISettingsService settings) => _settings = settings;

        public async Task<string> AskAsync(string userQuestion, string? activeCode = null)
        {
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                         ?? _settings.Settings.WorkspaceFolder; // ou champ dédié dans AppSettings

            var systemPrompt = "Tu es l'assistant officiel de Gamebuino AKA IDE. " +
                               "Tu aides le développeur à concevoir des jeux en C++ (PlatformIO / ESP-IDF) " +
                               "pour la console Gamebuino. Sois concis, donne des exemples en code propre et optimisé en RAM.";

            var prompt = string.IsNullOrWhiteSpace(activeCode)
                ? userQuestion
                : $"Code actuel:\n```cpp\n{activeCode}\n```\n\nQuestion de l'utilisateur: {userQuestion}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = systemPrompt + "\n\n" + prompt }
                        }
                    }
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
            var res = await _http.PostAsync(url, new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
            var json = await res.Content.ReadAsStringAsync();
            var parsed = JObject.Parse(json);
            return parsed["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() 
                   ?? "Impossible d'obtenir une réponse de l'assistant.";
        }

        public Task<string> ExplainBuildErrorAsync(string errorMessage, string codeContext)
        {
            return AskAsync($"Voici l'erreur de compilation:\n{errorMessage}\nExplique la cause et fournis la correction exacte.");
        }

        public async Task<string> ShareGistAsync(string fileName, string content, string description)
        {
            // Création d'un Gist anonyme ou avec token GitHub
            var body = new
            {
                description = description,
                @public = true,
                files = new System.Collections.Generic.Dictionary<string, object>
                {
                    { fileName, new { content = content } }
                }
            };
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.github.com/gists")
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            req.Headers.UserAgent.ParseAdd("GamebuinoAKA-IDE");
            var res = await _http.SendAsync(req);
            var resJson = await res.Content.ReadAsStringAsync();
            var parsed = JObject.Parse(resJson);
            return parsed["html_url"]?.ToString() ?? "Erreur lors de la publication du Gist.";
        }
    }
}
```
