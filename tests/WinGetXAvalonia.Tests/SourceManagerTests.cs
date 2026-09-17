// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.Sources;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class SourceManagerTests
{
    [Fact]
    public void Parse_SkipsHeaderAndSeparator_ReturnsSources()
    {
        const string output =
            "Name                             Arg                                                     Type\r\n" +
            "--------------------------------------------------------------------------------------------------\r\n" +
            "msstore                          https://storeedgefd.dsx.mp.microsoft.com/v9.0           Microsoft.Rest\r\n" +
            "winget                           https://cdn.winget.microsoft.com/cache                  Microsoft.Rest\r\n";

        var sources = WingetSourceParser.Parse(output);

        Assert.Equal(2, sources.Count);
        Assert.Equal("msstore", sources[0].Name);
        Assert.Equal("winget", sources[1].Name);
        Assert.True(sources[0].Enabled);
    }

    [Fact]
    public void Parse_EmptyOutput_ReturnsEmptyList()
    {
        Assert.Empty(WingetSourceParser.Parse(""));
    }

    [Fact]
    public async Task GetSourcesAsync_ParsesWingetOutput()
    {
        var winget = new StubWingetProcess(
            "Name  Arg  Type\r\nmsstore  https://example.com  Microsoft.Rest\r\n");

        var service = new SourceManagerService(winget);

        var sources = await service.GetSourcesAsync();

        Assert.Equal("source list", winget.LastCommand);
        Assert.Single(sources);
        Assert.Equal("msstore", sources[0].Name);
    }

    [Fact]
    public async Task AddAsync_ExecutesSourceAdd()
    {
        var winget = new StubWingetProcess("");

        var service = new SourceManagerService(winget);

        await service.AddAsync("myfeed", "https://example.com");

        Assert.Equal("source add -n myfeed https://example.com", winget.LastCommand);
    }

    [Fact]
    public async Task RemoveAsync_ExecutesSourceRemove()
    {
        var winget = new StubWingetProcess("");

        var service = new SourceManagerService(winget);

        await service.RemoveAsync("msstore");

        Assert.Equal("source remove msstore", winget.LastCommand);
    }

    [Fact]
    public async Task ResetAsync_ExecutesSourceReset()
    {
        var winget = new StubWingetProcess("");

        var service = new SourceManagerService(winget);

        await service.ResetAsync();

        Assert.Equal("source reset --force", winget.LastCommand);
    }

    [Fact]
    public async Task UpdateAsync_ExecutesSourceUpdate()
    {
        var winget = new StubWingetProcess("");

        var service = new SourceManagerService(winget);

        await service.UpdateAsync();

        Assert.Equal("source update", winget.LastCommand);
    }

    [Fact]
    public async Task InitializeAsync_LoadsSourcesAndReachesLoadedState()
    {
        var service = new StubSourceManagerService();
        var viewModel = new SourceManagerViewModel(service, new FakeDialogService());

        await viewModel.InitializeAsync();

        Assert.Equal(2, viewModel.Sources.Count);
        Assert.Equal(LoadState.Loaded, viewModel.State);
        Assert.True(viewModel.IsLoaded);
    }

    [Fact]
    public async Task Refresh_WhenServiceFails_ReachesErrorState()
    {
        var service = new FailingSourceManagerService();
        var viewModel = new SourceManagerViewModel(service, new FakeDialogService());

        await viewModel.Refresh();

        Assert.Empty(viewModel.Sources);
        Assert.Equal(LoadState.Error, viewModel.State);
        Assert.True(viewModel.HasError);
    }

    private sealed class StubWingetProcess : IWingetProcess
    {
        public StubWingetProcess(string output)
        {
            Output = output;
        }

        public string Output { get; }

        public string? LastCommand { get; private set; }

        public Task<string> ExecuteAsync(string arguments)
        {
            LastCommand = arguments;
            return Task.FromResult(Output);
        }
    }

    private sealed class StubSourceManagerService : ISourceManagerService
    {
        public Task<IReadOnlyList<WingetSource>> GetSourcesAsync()
        {
            return Task.FromResult<IReadOnlyList<WingetSource>>(
            [
                new WingetSource { Name = "winget" },
                new WingetSource { Name = "msstore" }
            ]);
        }

        public Task AddAsync(string name, string arg) => Task.CompletedTask;

        public Task RemoveAsync(string name) => Task.CompletedTask;

        public Task UpdateAsync() => Task.CompletedTask;

        public Task ResetAsync() => Task.CompletedTask;
    }

    private sealed class FailingSourceManagerService : ISourceManagerService
    {
        public Task<IReadOnlyList<WingetSource>> GetSourcesAsync()
        {
            throw new InvalidOperationException("boom");
        }

        public Task AddAsync(string name, string arg) => Task.CompletedTask;

        public Task RemoveAsync(string name) => Task.CompletedTask;

        public Task UpdateAsync() => Task.CompletedTask;

        public Task ResetAsync() => Task.CompletedTask;
    }
private sealed class FakeDialogService : IDialogService
    {
        public Task<bool> ConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null)
        {
            return Task.FromResult(true);
        }

        public Task ProgressAsync(string title, Func<IProgress<double>, Task> operation) => Task.CompletedTask;

        public Task ShowErrorAsync(string title, string message) => Task.CompletedTask;
    }
}
