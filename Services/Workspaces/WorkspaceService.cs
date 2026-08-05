// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Linq;
using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Workspaces;

public class WorkspaceService : IWorkspaceService
{
    private readonly string _folder;
    private readonly WorkspaceSerializer _serializer;
    private readonly WorkspaceImporter _importer;
    private readonly WorkspaceExporter _exporter;

    public WorkspaceService()
        : this(
            Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinGetX",
            "Workspaces"),
            new WorkspaceSerializer())
    {
    }

    public WorkspaceService(string folder)
        : this(folder, new WorkspaceSerializer())
    {
    }

    public WorkspaceService(string folder, WorkspaceSerializer serializer)
        : this(folder, new WorkspaceImporter(serializer), new WorkspaceExporter(serializer))
    {
    }

    public WorkspaceService(
        string folder,
        WorkspaceImporter importer,
        WorkspaceExporter exporter)
    {
        _folder = folder;
        _serializer = new WorkspaceSerializer();
        _importer = importer;
        _exporter = exporter;

        Directory.CreateDirectory(folder);
    }

    public async Task<IReadOnlyList<WorkspaceModel>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new List<WorkspaceModel>();

        foreach (var file in Directory.GetFiles(_folder, "*.workspace.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await using var stream = File.OpenRead(file);

                var workspace = await JsonSerializer.DeserializeAsync<WorkspaceModel>(
                    stream,
                    _serializer.Options,
                    cancellationToken);

                if (workspace is not null)
                    result.Add(workspace);
            }
            catch (Exception ex) when (ex is InvalidDataException or JsonException)
            {
                // bozuk dosya tüm listeyi bozmasın
            }
        }

        return result
            .OrderBy(x => x.Name)
            .ToList();
    }

    public async Task<WorkspaceModel?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var path = GetWorkspacePath(id);

        if (!File.Exists(path))
            return null;

        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<WorkspaceModel>(
            stream,
            _serializer.Options,
            cancellationToken);
    }

    public async Task SaveAsync(
        WorkspaceModel workspace,
        CancellationToken cancellationToken = default)
    {
        workspace.Modified = DateTime.Now;

        var path = GetWorkspacePath(workspace.Id);

        await using var stream = File.Create(path);

        await JsonSerializer.SerializeAsync(
            stream,
            workspace,
            _serializer.Options,
            cancellationToken);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = GetWorkspacePath(id);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(GetWorkspacePath(id)));
    }

    public async Task ImportAsync(string path, CancellationToken cancellationToken = default)
    {
        var workspace = await _importer.ImportAsync(path, cancellationToken);

        // içe aktarılan çalışma alanı mevcut dosyanın üzerine yazılmasın
        workspace.Id = Guid.NewGuid();
        workspace.Created = DateTime.Now;
        workspace.Modified = DateTime.Now;
        workspace.LastUsed = null;

        await SaveAsync(workspace, cancellationToken);
    }

    public async Task ExportAsync(
        Guid id,
        string path,
        CancellationToken cancellationToken = default)
    {
        var workspace = await GetAsync(id, cancellationToken)
            ?? throw new FileNotFoundException("Workspace not found.", GetWorkspacePath(id));

        await _exporter.ExportAsync(workspace, path, cancellationToken);
    }

    private string GetWorkspacePath(Guid id)
        => Path.Combine(_folder, $"{id}.workspace.json");
}
