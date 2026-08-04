// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.Json;
using WinGetXAvalonia.Infrastructure.IO;

namespace WinGetXAvalonia.Infrastructure.Persistence;

public sealed class JsonStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly FileSystemService _fileSystem;

    public JsonStorage(FileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public Task SaveAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default)
        => _fileSystem.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(value, Options),
            cancellationToken);

    public async Task<T?> LoadAsync<T>(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (!await _fileSystem.ExistsAsync(path, cancellationToken))
            return default;

        var json = await _fileSystem.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        => _fileSystem.DeleteAsync(path, cancellationToken);

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        => _fileSystem.ExistsAsync(path, cancellationToken);

    public async Task<IReadOnlyList<string>> EnumerateAsync(
        string directory,
        string searchPattern,
        CancellationToken cancellationToken = default)
        => await _fileSystem.EnumerateFilesAsync(directory, searchPattern, cancellationToken);
}
