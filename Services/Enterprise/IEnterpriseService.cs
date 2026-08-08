// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Enterprise;

public interface IEnterpriseService
{
    Task<IReadOnlyList<CompanyComputer>> GetComputersAsync(
        CancellationToken cancellationToken = default);

    Task<FleetStats> GetStatsAsync(
        CancellationToken cancellationToken = default);

    Task<RemoteCommand> SendCommandAsync(
        Guid computerId,
        RemoteCommandKind kind,
        string payload,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RemoteCommand>> GetCommandsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PolicyRule>> GetPoliciesAsync(
        CancellationToken cancellationToken = default);

    Task SavePolicyAsync(
        PolicyRule rule,
        CancellationToken cancellationToken = default);

    Task DeletePolicyAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PolicyViolation>> EvaluatePoliciesAsync(
        CancellationToken cancellationToken = default);
}
