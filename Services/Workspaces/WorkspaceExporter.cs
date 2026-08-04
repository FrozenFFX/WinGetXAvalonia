using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Workspaces;

public sealed class WorkspaceExporter
{
    private readonly WorkspaceSerializer _serializer;

    public WorkspaceExporter(WorkspaceSerializer serializer)
    {
        _serializer = serializer;
    }

    public Task ExportAsync(
        WorkspaceModel workspace,
        string path,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        return File.WriteAllTextAsync(path, _serializer.Serialize(workspace), cancellationToken);
    }
}
