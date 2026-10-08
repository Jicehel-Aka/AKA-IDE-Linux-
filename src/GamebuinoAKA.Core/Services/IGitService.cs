using System;
using System.Threading;
using System.Threading.Tasks;

namespace GamebuinoAKA.Core.Services
{
    public interface IGitService
    {
        Task<bool> IsInstalledAsync();
        Task<string> CloneAsync(string repoUrl, string? folderName, Action<string>? onOutput, CancellationToken ct = default);
    }
}
