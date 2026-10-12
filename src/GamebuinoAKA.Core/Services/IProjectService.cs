using System.Collections.Generic;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    public interface IProjectService
    {
        Task<List<GamebuinoProject>> ScanWorkspaceAsync();
        Task<List<GamebuinoProject>> GetRecentProjectsAsync();
        void DeleteProject(GamebuinoProject project);
    }
}
