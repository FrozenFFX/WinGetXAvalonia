// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Runtime;

namespace WinGetXAvalonia.Tests;

public class RuntimeHostTests
{
    [Fact]
    public async Task StartAsync_RunsStepsInDeclaredOrder()
    {
        var host = CreateHost();

        await host.StartAsync();

        Assert.Equal(
            new[] { "Configuration", "Themes", "Plugins", "Services", "Workspaces" },
            host.Steps);
    }

    [Fact]
    public async Task StartAsync_ReachesReadyState()
    {
        var host = CreateHost();

        await host.StartAsync();

        Assert.Equal(RuntimeState.Ready, host.Context.State);
        Assert.True(host.Context.IsReady);
    }

    [Fact]
    public async Task StartAsync_FiresReadyEvent()
    {
        var host = CreateHost();
        var fired = 0;
        host.Ready += (_, _) => fired++;

        await host.StartAsync();

        Assert.Equal(1, fired);
    }

    [Fact]
    public async Task StartAsync_IsIdempotent()
    {
        var host = CreateHost();

        await host.StartAsync();
        await host.StartAsync();

        Assert.Equal(5, host.Steps.Count);
    }

    [Fact]
    public async Task StartAsync_RaisesStateChanges()
    {
        var host = CreateHost();
        var states = new List<RuntimeState>();
        host.Context.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RuntimeContext.State))
                states.Add(host.Context.State);
        };

        await host.StartAsync();

        Assert.Equal(RuntimeState.Loading, states[0]);
        Assert.Equal(RuntimeState.Ready, states[^1]);
    }

    [Fact]
    public async Task ShutdownAsync_SetsShuttingDownAndFiresEvents()
    {
        var host = CreateHost();
        await host.StartAsync();
        var shuttingDown = 0;
        var completed = 0;
        host.ShuttingDown += (_, _) => shuttingDown++;
        host.ShutdownCompleted += (_, _) => completed++;

        await host.ShutdownAsync();

        Assert.Equal(RuntimeState.ShuttingDown, host.Context.State);
        Assert.Equal(1, shuttingDown);
        Assert.Equal(1, completed);
    }

    [Fact]
    public async Task SetBusy_FlipsBetweenBusyAndReady()
    {
        var host = CreateHost();
        await host.StartAsync();

        host.SetBusy(true);

        Assert.Equal(RuntimeState.Busy, host.Context.State);
        Assert.True(host.Context.IsBusy);

        host.SetBusy(false);

        Assert.Equal(RuntimeState.Ready, host.Context.State);
    }

    [Fact]
    public void Context_StartsWithStartingStateAndUptime()
    {
        var context = new RuntimeContext();

        Assert.Equal(RuntimeState.Starting, context.State);
        Assert.False(context.IsReady);
        Assert.False(context.IsBusy);
        Assert.Equal("Starting", context.StateText);
        Assert.True(context.Uptime >= TimeSpan.Zero);
    }

    private static RecordingHost CreateHost()
        => new(new RuntimeContext(), new RuntimeFeatureManager());

    private sealed class RecordingHost : RuntimeHost
    {
        public RecordingHost(RuntimeContext context, RuntimeFeatureManager features)
            : base(context, features)
        {
        }

        public List<string> Steps { get; } = new();

        protected override Task LoadConfigurationAsync()
        {
            Steps.Add("Configuration");
            return Task.CompletedTask;
        }

        protected override Task LoadThemesAsync()
        {
            Steps.Add("Themes");
            return Task.CompletedTask;
        }

        protected override Task LoadPluginsAsync()
        {
            Steps.Add("Plugins");
            return Task.CompletedTask;
        }

        protected override Task InitializeServicesAsync()
        {
            Steps.Add("Services");
            return Task.CompletedTask;
        }

        protected override Task RestoreWorkspacesAsync()
        {
            Steps.Add("Workspaces");
            return Task.CompletedTask;
        }
    }
}
