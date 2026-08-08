// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Enterprise;

namespace WinGetXAvalonia.Tests;

public class EnterpriseServiceTests
{
    private readonly EnterpriseService _service = new();

    [Fact]
    public async Task GetComputers_UsesLocalMachineOnly()
    {
        var computers = await _service.GetComputersAsync();

        Assert.NotEmpty(computers);
        var computer = Assert.Single(computers);
        Assert.True(computer.Online);
        Assert.Equal(Environment.MachineName, computer.Name);
        Assert.NotEmpty(computer.Os);
        Assert.NotEmpty(computer.Cpu);
        Assert.NotEmpty(computer.Ram);
    }

    [Fact]
    public async Task GetStats_ComputesTotals()
    {
        var stats = await _service.GetStatsAsync();
        var computers = await _service.GetComputersAsync();

        Assert.Equal(computers.Count, stats.Total);
        Assert.Equal(computers.Count(c => c.Online), stats.Online);
        Assert.Equal(computers.Count(c => !c.Online), stats.Offline);
        Assert.Equal(computers.Sum(c => c.PendingUpdates), stats.PendingUpdates);
        Assert.Equal(computers.Sum(c => c.CriticalUpdates), stats.CriticalUpdates);
    }

    [Fact]
    public async Task SendCommand_ToOnlineComputer_Succeeds()
    {
        var computers = await _service.GetComputersAsync();
        var target = computers.First(c => c.Online);

        var command = await _service.SendCommandAsync(
            target.Id, RemoteCommandKind.InstallPackage, "Google Chrome");

        Assert.Equal(RemoteCommandStatus.Success, command.Status);
        Assert.Contains(command.Payload, target.InstalledApps);
        Assert.Equal("Google Chrome", command.Payload);
    }

    [Fact]
    public async Task SendCommand_UnknownComputer_Fails()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SendCommandAsync(
                Guid.NewGuid(),
                RemoteCommandKind.InstallPackage,
                "Git"));
    }

    [Fact]
    public async Task SendCommand_UnknownComputer_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SendCommandAsync(Guid.NewGuid(), RemoteCommandKind.Restart, "x"));
    }

    [Fact]
    public async Task Restart_RecyclesConnection()
    {
        var computers = await _service.GetComputersAsync();
        var target = computers.First(c => c.Online);

        var command = await _service.SendCommandAsync(
            target.Id, RemoteCommandKind.Restart, "restart");

        Assert.Equal(RemoteCommandStatus.Success, command.Status);
        Assert.True(target.Online);
    }

    [Fact]
    public async Task Commands_AreLogged()
    {
        var computers = await _service.GetComputersAsync();
        var target = computers.First(c => c.Online);

        await _service.SendCommandAsync(target.Id, RemoteCommandKind.PowerShell, "Get-Process");

        var commands = await _service.GetCommandsAsync();

        Assert.NotEmpty(commands);
        Assert.Equal(target.Name, commands[0].ComputerName);
    }

    [Fact]
    public async Task Policies_CanBeSavedAndDeleted()
    {
        var rule = new PolicyRule
        {
            Name = "No Tor",
            Action = PolicyAction.BlockInstall,
            Package = "Tor"
        };

        await _service.SavePolicyAsync(rule);

        var policies = await _service.GetPoliciesAsync();
        Assert.Contains(policies, p => p.Name == "No Tor");

        await _service.DeletePolicyAsync(rule.Id);

        policies = await _service.GetPoliciesAsync();
        Assert.DoesNotContain(policies, p => p.Id == rule.Id);
    }

    [Fact]
    public async Task EvaluatePolicies_NoFakeInstalledApps_ReturnsEmpty()
    {
        var rule = new PolicyRule
        {
            Name = "Steam is banned",
            Action = PolicyAction.BlockInstall,
            Package = "Steam",
            Enabled = true
        };

        await _service.SavePolicyAsync(rule);

        var violations = await _service.EvaluatePoliciesAsync();

        Assert.Empty(violations);
    }

    [Fact]
    public async Task EvaluatePolicies_DisabledRule_IsIgnored()
    {
        var rule = new PolicyRule
        {
            Name = "Disabled rule",
            Action = PolicyAction.BlockInstall,
            Package = "Steam",
            Enabled = false
        };

        await _service.SavePolicyAsync(rule);

        var violations = await _service.EvaluatePoliciesAsync();

        Assert.DoesNotContain(violations, v => v.RuleName == "Disabled rule");
    }
}
