// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Identity;

public interface IIdentityService
{
    Task<IReadOnlyList<SoftwareIdentity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SoftwareIdentity?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SoftwareIdentity?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        SoftwareIdentity identity,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SoftwareIdentity> RecordInstallAsync(
        SoftwareIdentity identity,
        CancellationToken cancellationToken = default);

    Task RecordUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task RecordRollbackAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
