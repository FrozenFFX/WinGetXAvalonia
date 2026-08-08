// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Blueprints;

public interface IBlueprintStore
{
    Task<IReadOnlyList<BlueprintModel>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        BlueprintModel blueprint,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BlueprintModel>> ImportJsonAsync(
        string json,
        CancellationToken cancellationToken = default);
}
