using System.Linq;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class ExampleCatalogServiceTests
    {
        [Fact]
        public void GetExamples_ReturnsBundledExamples()
        {
            var svc = new ExampleCatalogService();
            var ex = svc.GetExamples();
            Assert.NotEmpty(ex);
            Assert.Contains(ex, e => e.Id == "pong");
            Assert.Contains(ex, e => e.Id == "calculatrice");
            Assert.All(ex, e => Assert.False(string.IsNullOrWhiteSpace(e.Title)));
        }

        [Fact]
        public void ReadSource_ReturnsNonEmptySource_ForKnownExample()
        {
            var svc = new ExampleCatalogService();
            var pong = svc.GetExamples().First(e => e.Id == "pong");
            var src = svc.ReadSource(pong);
            Assert.False(string.IsNullOrWhiteSpace(src));
            Assert.True(src.Length > 200);   // un vrai script, pas un stub vide
        }

        [Fact]
        public void GetExamples_IsCached()
        {
            var svc = new ExampleCatalogService();
            Assert.Same(svc.GetExamples(), svc.GetExamples());
        }
    }
}
