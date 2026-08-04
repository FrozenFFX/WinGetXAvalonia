// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Workspaces;

public interface IWorkspaceService
{
    Task<IReadOnlyList<WorkspaceModel>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<WorkspaceModel?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        WorkspaceModel workspace,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task ImportAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task ExportAsync(
        Guid id,
        string path,
        CancellationToken cancellationToken = default);
}
