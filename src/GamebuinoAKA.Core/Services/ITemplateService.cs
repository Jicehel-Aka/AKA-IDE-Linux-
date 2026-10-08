using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    public interface ITemplateService
    {
        Task CreateProjectAsync(string projectName, string template, string destinationFolder, BuildSystem buildSystem, bool withAudio = false);
    }
}
