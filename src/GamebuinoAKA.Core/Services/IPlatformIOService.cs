using System.Threading.Tasks;

namespace GamebuinoAKA.Core.Services
{
    public interface IPlatformIOService : IBuildBackend
    {
        Task<string> GetVersionAsync();
        string DetectPioPath();
    }
}
