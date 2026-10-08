using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamebuinoAKA.Core.Services
{
    public sealed record FileFilter(string Name, IReadOnlyList<string> Extensions);

    public interface IFileDialogService
    {
        Task<string?> OpenFileAsync(string title, IReadOnlyList<FileFilter>? filters = null);
        Task<string?> SaveFileAsync(string title, string? suggestedName = null, IReadOnlyList<FileFilter>? filters = null);
        Task<string?> OpenFolderAsync(string title, string? startPath = null);
    }
}
