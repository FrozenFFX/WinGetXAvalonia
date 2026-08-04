using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Workspaces;

public sealed class WorkspaceSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public string Serialize(WorkspaceModel workspace)
        => JsonSerializer.Serialize(workspace, Options);

    public WorkspaceModel Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<WorkspaceModel>(json, Options)
                ?? throw new InvalidDataException("Invalid workspace file.");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Invalid workspace file.", ex);
        }
    }
}
