using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Studio;

public interface IStudioService
{
    Task<IReadOnlyList<StudioTool>> GetToolsAsync();
}
