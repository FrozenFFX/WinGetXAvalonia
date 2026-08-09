// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.IO;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Diagnostics;
using WinGetXAvalonia.Services.Performance;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class DiagnosticsTests
{
    [Fact]
    public async Task CreateReportAsync_PopulatesAllFields()
    {
        var service = new DiagnosticsService(new SystemInfoService());

        var report = await service.CreateReportAsync();

        Assert.False(string.IsNullOrWhiteSpace(report.WindowsVersion));
        Assert.False(string.IsNullOrWhiteSpace(report.DotNetVersion));
        Assert.True(report.Is64Bit);
        Assert.True(report.ProcessorCount > 0);
        Assert.False(string.IsNullOrWhiteSpace(report.MachineName));
        Assert.False(string.IsNullOrWhiteSpace(report.UserName));
        Assert.False(string.IsNullOrWhiteSpace(report.Architecture));
    }

    [Fact]
    public async Task CreateReportAsync_WingetInstalledMatchesFileSystem()
    {
        var service = new DiagnosticsService(new SystemInfoService());

        var report = await service.CreateReportAsync();

        string localAppData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        var expected = File.Exists(
            Path.Combine(localAppData, "Microsoft", "WindowsApps", "winget.exe"));

        Assert.Equal(expected, report.WingetInstalled);
    }

    [Fact]
    public async Task InitializeAsync_LoadsReportAndReachesLoadedState()
    {
        var service = new StubDiagnosticsService();
        var viewModel = new DiagnosticsViewModel(service);

        await viewModel.InitializeAsync();

        Assert.NotNull(viewModel.Report);
        Assert.Equal("TestWindows", viewModel.Report!.WindowsVersion);
        Assert.Equal(LoadState.Loaded, viewModel.State);
        Assert.True(viewModel.IsLoaded);
    }

    [Fact]
    public async Task Refresh_WhenServiceFails_ReachesErrorState()
    {
        var service = new FailingDiagnosticsService();
        var viewModel = new DiagnosticsViewModel(service);

        await viewModel.Refresh();

        Assert.Null(viewModel.Report);
        Assert.Equal(LoadState.Error, viewModel.State);
        Assert.True(viewModel.HasError);
        Assert.False(string.IsNullOrWhiteSpace(viewModel.ErrorMessage));
    }

    private sealed class StubDiagnosticsService : IDiagnosticsService
    {
        public Task<DiagnosticsReport> CreateReportAsync()
        {
            return Task.FromResult(new DiagnosticsReport
            {
                WindowsVersion = "TestWindows",
                DotNetVersion = "1.0.0",
                Is64Bit = true,
                ProcessorCount = 4,
                MachineName = "TestMachine",
                UserName = "TestUser",
                WingetInstalled = false
            });
        }
    }

    private sealed class FailingDiagnosticsService : IDiagnosticsService
    {
        public Task<DiagnosticsReport> CreateReportAsync()
        {
            throw new InvalidOperationException("boom");
        }
    }
}
