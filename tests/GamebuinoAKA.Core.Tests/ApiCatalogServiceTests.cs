using System.Linq;
using GamebuinoAKA.Core.Models;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class ApiCatalogServiceTests
    {
        [Fact]
        public void MicroPython_Catalog_ContainsAkaSymbols()
        {
            var svc = new ApiCatalogService();
            var syms = svc.GetSymbols(ProjectKind.MicroPython);
            Assert.NotEmpty(syms);
            Assert.Contains(syms, s => s.Name == "aka.fill_rect");
            Assert.Contains(syms, s => s.Name == "aka.update");
            Assert.Contains(syms, s => s.Name == "aka.UP" && s.Kind == "constant");
            // Chaque symbole a une signature non vide.
            Assert.All(syms, s => Assert.False(string.IsNullOrWhiteSpace(s.Signature)));
        }

        [Fact]
        public void Love2D_Catalog_ContainsLoveSymbols()
        {
            var svc = new ApiCatalogService();
            var syms = svc.GetSymbols(ProjectKind.Love2D);
            Assert.NotEmpty(syms);
            Assert.Contains(syms, s => s.Name == "love.update" && s.Kind == "callback");
            Assert.Contains(syms, s => s.Name == "love.graphics.rectangle");
        }

        [Fact]
        public void NonScriptKind_ReturnsEmpty()
        {
            var svc = new ApiCatalogService();
            Assert.Empty(svc.GetSymbols(ProjectKind.PlatformIO));
            Assert.Empty(svc.GetSymbols(ProjectKind.EspIdfNative));
        }

        [Fact]
        public void Catalog_IsCached_SameInstanceReturned()
        {
            var svc = new ApiCatalogService();
            var a = svc.GetSymbols(ProjectKind.MicroPython);
            var b = svc.GetSymbols(ProjectKind.MicroPython);
            Assert.Same(a, b);
        }
    }
}
