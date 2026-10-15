using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    /// <summary>
    /// Charge les catalogues d'API depuis <c>&lt;base&gt;/Api/*.json</c> (embarqués en
    /// Content, copiés dans la sortie de l'App ET des tests). Résultat mis en cache.
    /// </summary>
    public sealed class ApiCatalogService : IApiCatalogService
    {
        private static readonly IReadOnlyList<ApiSymbol> Empty = new List<ApiSymbol>();
        private readonly Dictionary<ProjectKind, IReadOnlyList<ApiSymbol>> _cache = new();

        public IReadOnlyList<ApiSymbol> GetSymbols(ProjectKind kind)
        {
            if (_cache.TryGetValue(kind, out var cached)) return cached;

            var file = kind switch
            {
                ProjectKind.Love2D      => "love2d-aka.json",
                ProjectKind.MicroPython => "micropython-aka.json",
                _ => null
            };

            IReadOnlyList<ApiSymbol> result = Empty;
            if (file != null)
            {
                try
                {
                    var path = Path.Combine(AppContext.BaseDirectory, "Api", file);
                    if (File.Exists(path))
                        result = JsonConvert.DeserializeObject<List<ApiSymbol>>(File.ReadAllText(path))
                                 ?? (List<ApiSymbol>)Empty;
                }
                catch { result = Empty; }
            }

            _cache[kind] = result;
            return result;
        }
    }
}
