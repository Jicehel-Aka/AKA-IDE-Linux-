using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Models;

namespace GamebuinoAKA.Core.Platform
{
    public sealed record ProcessRequest(string FileName, IReadOnlyList<string> Arguments, string? WorkingDirectory = null);

    public interface IProcessRunner
    {
        Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken = default);
        Task<int> RunStreamingAsync(ProcessRequest request, Action<string>? onOutput, CancellationToken cancellationToken = default);
    }
}
