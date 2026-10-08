using System.Threading.Tasks;

namespace GamebuinoAKA.Core.Services
{
    public interface IEspIdfService : IBuildBackend
    {
        Task<string> GetVersionAsync();
        string[] DetectSerialPorts();
    }
}
