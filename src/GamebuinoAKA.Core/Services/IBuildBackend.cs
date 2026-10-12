using System;
using System.Threading;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Services
{
    public interface IBuildBackend
    {
        Task BuildAsync(GamebuinoProject project, Action<string>? onOutput, CancellationToken ct = default);
        Task FlashAsync(GamebuinoProject project, Action<string>? onOutput, CancellationToken ct = default);
        Task MonitorAsync(GamebuinoProject project, Action<string>? onOutput, CancellationToken ct = default);
        Task CleanAsync(GamebuinoProject project, Action<string>? onOutput, CancellationToken ct = default);
    }
}
