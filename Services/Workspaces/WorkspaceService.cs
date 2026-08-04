using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Workspaces;

public class WorkspaceService : IWorkspaceService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    private readonly string _folder;

    public WorkspaceService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinGetX",
            "Workspaces"))
    {
    }

    public WorkspaceService(string folder)
    {
        _folder = folder;

        Directory.CreateDirectory(folder);
    }

    public async Task<IReadOnlyList<WorkspaceModel>> GetAllAsync()
    {
        var result = new List<WorkspaceModel>();

        foreach (var file in Directory.GetFiles(_folder, "*.workspace.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);

                var workspace = JsonSerializer.Deserialize<WorkspaceModel>(json);

                if (workspace != null)
                    result.Add(workspace);
            }
            catch (JsonException)
            {
                // bozuk dosya tüm listeyi bozmasın
            }
        }

        return result;
    }

    public async Task SaveAsync(WorkspaceModel workspace)
    {
        var path = Path.Combine(_folder, $"{workspace.Id}.workspace.json");

        var json = JsonSerializer.Serialize(workspace, Options);

        await File.WriteAllTextAsync(path, json);
    }

    public Task DeleteAsync(Guid id)
    {
        var file = Path.Combine(_folder, $"{id}.workspace.json");

        if (File.Exists(file))
            File.Delete(file);

        return Task.CompletedTask;
    }

    public async Task ImportAsync(string file)
    {
        var json = await File.ReadAllTextAsync(file);

        WorkspaceModel workspace;

        try
        {
            workspace = JsonSerializer.Deserialize<WorkspaceModel>(json)
                ?? throw new InvalidDataException("Invalid workspace file.");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Invalid workspace file.", ex);
        }

        // içe aktarılan çalışma alanı mevcut dosyanın üzerine yazılmasın
        workspace.Id = Guid.NewGuid();
        workspace.Created = DateTime.Now;
        workspace.LastUsed = DateTime.Now;

        await SaveAsync(workspace);
    }

    public async Task ExportAsync(Guid id, string file)
    {
        var path = Path.Combine(_folder, $"{id}.workspace.json");

        if (!File.Exists(path))
            throw new FileNotFoundException("Workspace not found.", path);

        var json = await File.ReadAllTextAsync(path);

        await File.WriteAllTextAsync(file, json);
    }
}
