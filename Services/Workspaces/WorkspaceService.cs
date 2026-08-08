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

    public async Task ImportJsonAsync(string json, CancellationToken cancellationToken = default)
    {
        var workspace = await _importer.ImportJsonAsync(json, cancellationToken);

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

    public async Task EnsureExampleWorkspacesAsync(
        CancellationToken cancellationToken = default)
    {
        var marker = Path.Combine(_folder, ".examples-seeded");

        if (File.Exists(marker))
            return;

        foreach (var preset in CreateExampleWorkspaces())
            await SaveAsync(preset, cancellationToken);

        await File.WriteAllTextAsync(
            marker,
            DateTime.Now.ToString("O"),
            cancellationToken);
    }

    public static IReadOnlyList<WorkspaceModel> CreateExampleWorkspaces()
    {
        var gaming = Preset("Gaming", "Essential gaming setup", "🎮",
            ("Valve.Steam", "Steam"),
            ("EpicGames.EpicGamesLauncher", "Epic Games Launcher"),
            ("Discord.Discord", "Discord"),
            ("OBSProject.OBSStudio", "OBS Studio"));

        var developer = Preset("Developer", "Coding tools and runtimes", "💻",
            ("Git.Git", "Git"),
            ("Microsoft.VisualStudioCode", "Visual Studio Code"),
            ("Microsoft.PowerShell", "PowerShell"),
            ("GitHub.cli", "GitHub CLI"),
            ("Docker.DockerDesktop", "Docker Desktop"),
            ("OpenJS.NodeJS.LTS", "Node.js LTS"));

        var designer = Preset("Designer", "Creative and design tools", "🎨",
            ("Figma.Figma", "Figma"),
            ("BlenderFoundation.Blender", "Blender"),
            ("GIMP.GIMP", "GIMP"),
            ("Inkscape.Inkscape", "Inkscape"));

        return new[] { gaming, developer, designer };
    }

    private static WorkspaceModel Preset(
        string name,
        string description,
        string icon,
        params (string Id, string Name)[] packages)
    {
        var workspace = new WorkspaceModel
        {
            Name = name,
            Description = description,
            Icon = icon,
            Favorite = true
        };

        foreach (var (id, packageName) in packages)
        {
            workspace.Packages.Add(new WorkspacePackage
            {
                Id = id,
                Name = packageName,
                Version = "2.0.0",
                Source = "winget"
            });
        }

        return workspace;
    }

    private string GetWorkspacePath(Guid id)
        => Path.Combine(_folder, $"{id}.workspace.json");
}
