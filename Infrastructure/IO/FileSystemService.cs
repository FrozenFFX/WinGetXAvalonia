namespace WinGetXAvalonia.Infrastructure.IO;

public sealed class FileSystemService
{
    public async Task<string> ReadAllTextAsync(
        string path,
        CancellationToken cancellationToken = default)
        => await File.ReadAllTextAsync(path, cancellationToken);

    public async Task WriteAllTextAsync(
        string path,
        string contents,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, contents, cancellationToken);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(path));
    }

    public Task<IReadOnlyList<string>> EnumerateFilesAsync(
        string directory,
        string searchPattern,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(directory))
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

        return Task.FromResult<IReadOnlyList<string>>(
            Directory.EnumerateFiles(directory, searchPattern).ToList());
    }
}
