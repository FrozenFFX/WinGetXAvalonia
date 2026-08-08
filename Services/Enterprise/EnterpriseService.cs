// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Enterprise;

public sealed class EnterpriseService : IEnterpriseService
{
    private readonly List<CompanyComputer> _computers = new();
    private readonly List<RemoteCommand> _commands = new();
    private readonly List<PolicyRule> _policies = new();

    public EnterpriseService()
    {
        SeedComputers();
        SeedPolicies();
    }

    public Task<IReadOnlyList<CompanyComputer>> GetComputersAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<CompanyComputer>>(_computers);
    }

    public Task<FleetStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new FleetStats
        {
            Total = _computers.Count,
            Online = _computers.Count(c => c.Online),
            Offline = _computers.Count(c => !c.Online),
            PendingUpdates = _computers.Sum(c => c.PendingUpdates),
            CriticalUpdates = _computers.Sum(c => c.CriticalUpdates),
            FailedInstallations = _computers.Sum(c => c.FailedInstallations)
        });
    }

    public async Task<RemoteCommand> SendCommandAsync(
        Guid computerId,
        RemoteCommandKind kind,
        string payload,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var computer = _computers.FirstOrDefault(c => c.Id == computerId)
            ?? throw new InvalidOperationException("Computer not found.");

        var command = new RemoteCommand
        {
            ComputerName = computer.Name,
            Kind = kind,
            Payload = payload
        };

        _commands.Insert(0, command);

        await SimulateAsync(computer, command);

        return command;
    }

    public Task<IReadOnlyList<RemoteCommand>> GetCommandsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<RemoteCommand>>(_commands);
    }

    public Task<IReadOnlyList<PolicyRule>> GetPoliciesAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<PolicyRule>>(_policies);
    }

    public Task SavePolicyAsync(
        PolicyRule rule,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existing = _policies.FirstOrDefault(p => p.Id == rule.Id);

        if (existing is not null)
        {
            existing.Name = rule.Name;
            existing.Action = rule.Action;
            existing.Package = rule.Package;
            existing.Enabled = rule.Enabled;
            existing.Description = rule.Description;
        }
        else
        {
            _policies.Add(rule);
        }

        return Task.CompletedTask;
    }

    public Task DeletePolicyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _policies.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PolicyViolation>> EvaluatePoliciesAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var violations = new List<PolicyViolation>();

        foreach (var rule in _policies.Where(p => p.Enabled))
        {
            foreach (var computer in _computers)
            {
                var installed = computer.InstalledApps;
                var hasPackage = installed.Any(a =>
                    a.Contains(rule.Package, StringComparison.OrdinalIgnoreCase));

                switch (rule.Action)
                {
                    case PolicyAction.BlockUninstall:
                        if (hasPackage)
                            violations.Add(new PolicyViolation
                            {
                                ComputerName = computer.Name,
                                RuleName = rule.Name,
                                Package = rule.Package,
                                Message = $"'{rule.Package}' is protected and cannot be removed."
                            });
                        break;

                    case PolicyAction.BlockInstall:
                        if (hasPackage)
                            violations.Add(new PolicyViolation
                            {
                                ComputerName = computer.Name,
                                RuleName = rule.Name,
                                Package = rule.Package,
                                Message = $"'{rule.Package}' is blocked by policy but is installed."
                            });
                        break;

                    case PolicyAction.AllowOnly:
                        var notAllowed = installed.Where(a =>
                            !a.Contains(rule.Package, StringComparison.OrdinalIgnoreCase)
                            && !IsSystemAllowed(a)).ToList();

                        foreach (var app in notAllowed)
                        {
                            violations.Add(new PolicyViolation
                            {
                                ComputerName = computer.Name,
                                RuleName = rule.Name,
                                Package = app,
                                Message = $"'{app}' is not in the allow-list."
                            });
                        }
                        break;
                }
            }
        }

        return Task.FromResult<IReadOnlyList<PolicyViolation>>(violations);
    }

    private async Task SimulateAsync(CompanyComputer computer, RemoteCommand command)
    {
        if (!computer.Online)
        {
            command.Status = RemoteCommandStatus.Failed;
            return;
        }

        computer.IsProcessing = true;

        await Task.Delay(150);

        computer.IsProcessing = false;

        switch (command.Kind)
        {
            case RemoteCommandKind.InstallPackage:
                computer.InstalledApps.Add(command.Payload);
                computer.PendingUpdates += 1;
                break;

            case RemoteCommandKind.Restart:
                computer.Online = false;
                command.Status = RemoteCommandStatus.Success;
                await Task.Delay(120);
                computer.Online = true;
                computer.LastSeen = DateTime.Now;
                break;

            case RemoteCommandKind.PowerShell:
                break;
        }

        command.Status = RemoteCommandStatus.Success;
        command.Timestamp = DateTime.Now;
    }

    private void SeedComputers()
    {
        _computers.Add(new CompanyComputer
        {
            Name = Environment.MachineName,
            Os = GetOsLabel(),
            Cpu = $"{Environment.ProcessorCount} threads",
            Ram = GetRamLabel(),
            Online = true,
            LastSeen = DateTime.Now,
            PendingUpdates = 0,
            CriticalUpdates = 0,
            FailedInstallations = 0,
            InstalledApps = new ObservableCollection<string>()
        });
    }

    private static string GetOsLabel()
    {
        if (!OperatingSystem.IsWindows())
            return Environment.OSVersion.VersionString;

        var version = Environment.OSVersion.Version;
        var family = version.Build >= 22000 ? "Windows 11" : "Windows 10";

        return $"{family} {RuntimeInformation.OSArchitecture}";
    }

    private static string GetRamLabel()
    {
        var status = new MemoryStatusEx();

        if (!GlobalMemoryStatusEx(status) || status.TotalPhysicalMemory == 0)
            return "N/A";

        var gb = status.TotalPhysicalMemory / 1024d / 1024d / 1024d;

        return $"{gb:0.#} GB";
    }

    private void SeedPolicies()
    {
        _policies.AddRange(
        [
            new PolicyRule
            {
                Name = "Chrome must stay",
                Action = PolicyAction.BlockUninstall,
                Package = "Chrome",
                Enabled = true,
                Description = "Google Chrome cannot be removed on managed devices."
            },
            new PolicyRule
            {
                Name = "No Steam",
                Action = PolicyAction.BlockInstall,
                Package = "Steam",
                Enabled = true,
                Description = "Steam is not allowed on company computers."
            },
            new PolicyRule
            {
                Name = "Discord restricted",
                Action = PolicyAction.BlockInstall,
                Package = "Discord",
                Enabled = true,
                Description = "Discord is blocked on the company network."
            },
            new PolicyRule
            {
                Name = "Approved software only",
                Action = PolicyAction.AllowOnly,
                Package = "Store",
                Enabled = false,
                Description = "Only Microsoft Store and allow-listed apps."
            }
        ]);
    }

    private static bool IsSystemAllowed(string app)
    {
        string[] allowed =
        [
            "Microsoft Store", "PowerShell", "Microsoft Teams", "Outlook", "Excel", "Word",
            "PowerToys", "7-Zip", "Windows Terminal", "Git", "Visual Studio Code", "Chrome"
        ];

        return allowed.Any(a => app.Contains(a, StringComparison.OrdinalIgnoreCase));
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MemoryStatusEx
    {
        public uint Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
        public uint MemoryLoad;
        public ulong TotalPhysicalMemory;
        public ulong AvailablePhysicalMemory;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx status);
}
