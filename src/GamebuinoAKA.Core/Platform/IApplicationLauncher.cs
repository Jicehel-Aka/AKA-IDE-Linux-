namespace GamebuinoAKA.Core.Platform;

public interface IApplicationLauncher
{
    Task OpenFolderAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task OpenFileAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task LaunchVSCodeAsync(
        string workspace,
        CancellationToken cancellationToken = default);
}
