using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Workspaces;

public interface IWorkspaceService
{
    Task<IReadOnlyList<WorkspaceModel>> GetAllAsync();

    Task SaveAsync(WorkspaceModel workspace);

    Task DeleteAsync(Guid id);

    Task ImportAsync(string file);

    Task ExportAsync(Guid id, string file);
}
