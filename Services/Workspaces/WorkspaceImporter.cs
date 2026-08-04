using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Workspaces;

public sealed class WorkspaceImporter
{
    private readonly WorkspaceSerializer _serializer;

    public WorkspaceImporter(WorkspaceSerializer serializer)
    {
        _serializer = serializer;
    }

    public async Task<WorkspaceModel> ImportAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return _serializer.Deserialize(json);
    }
}
