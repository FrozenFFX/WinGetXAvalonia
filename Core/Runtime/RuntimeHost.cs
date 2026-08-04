// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.Runtime;

public class RuntimeHost
{
    private readonly RuntimeContext _context;
    private readonly RuntimeFeatureManager _features;
    private bool _started;

    public RuntimeHost(RuntimeContext context, RuntimeFeatureManager features)
    {
        _context = context;
        _features = features;
    }

    public RuntimeContext Context => _context;

    public RuntimeFeatureManager Features => _features;

    public RuntimeState State => _context.State;

    public event EventHandler? Starting;

    public event EventHandler? Ready;

    public event EventHandler? ShuttingDown;

    public event EventHandler? ShutdownCompleted;

    public async Task StartAsync()
    {
        if (_started)
            return;

        _started = true;

        _context.State = RuntimeState.Starting;
        Starting?.Invoke(this, EventArgs.Empty);

        _context.State = RuntimeState.Loading;
        await LoadConfigurationAsync();
        await LoadThemesAsync();
        await LoadPluginsAsync();
        await InitializeServicesAsync();
        await RestoreWorkspacesAsync();

        _context.State = RuntimeState.Ready;
        Ready?.Invoke(this, EventArgs.Empty);
    }

    public async Task ShutdownAsync()
    {
        _context.State = RuntimeState.ShuttingDown;
        ShuttingDown?.Invoke(this, EventArgs.Empty);

        await DisposeServicesAsync();

        ShutdownCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void SetBusy(bool busy)
        => _context.State = busy ? RuntimeState.Busy : RuntimeState.Ready;

    protected virtual Task LoadConfigurationAsync() => Task.CompletedTask;

    protected virtual Task LoadThemesAsync() => Task.CompletedTask;

    protected virtual Task LoadPluginsAsync() => Task.CompletedTask;

    protected virtual Task InitializeServicesAsync() => Task.CompletedTask;

    protected virtual Task RestoreWorkspacesAsync() => Task.CompletedTask;

    protected virtual Task DisposeServicesAsync() => Task.CompletedTask;
}
