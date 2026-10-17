using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Charge les exemples depuis <c>&lt;base&gt;/Examples/manifest.json</c> et lit leurs
    /// sources sous <c>&lt;base&gt;/Examples/</c> (embarqués en Content). Résultat mis en cache.
    /// </summary>
    public sealed class ExampleCatalogService : IExampleCatalogService
    {
        private IReadOnlyList<ExampleInfo>? _cache;

        private static string BaseDir => Path.Combine(AppContext.BaseDirectory, "Examples");

        public IReadOnlyList<ExampleInfo> GetExamples()
        {
            if (_cache != null) return _cache;
            var list = new List<ExampleInfo>();
            try
            {
                var manifest = Path.Combine(BaseDir, "manifest.json");
                if (File.Exists(manifest))
                    list = JsonConvert.DeserializeObject<List<ExampleInfo>>(File.ReadAllText(manifest)) ?? new();
            }
            catch { list = new(); }
            _cache = list;
            return _cache;
        }

        public string ReadSource(ExampleInfo example)
        {
            if (example == null || string.IsNullOrEmpty(example.File)) return "";
            try
            {
                var path = Path.Combine(BaseDir, example.File.Replace('/', Path.DirectorySeparatorChar));
                return File.Exists(path) ? File.ReadAllText(path) : "";
            }
            catch { return ""; }
        }
    }
}
