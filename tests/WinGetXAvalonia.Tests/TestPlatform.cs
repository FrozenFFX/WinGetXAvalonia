// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Threading;

namespace WinGetXAvalonia.Tests;

[CollectionDefinition("UiDispatcher", DisableParallelization = true)]
public sealed class UiDispatcherCollection;

public static class TestPlatform
{
    private static readonly Lazy<object> _ui = new(StartUiThread, LazyThreadSafetyMode.ExecutionAndPublication);

    [ModuleInitializer]
    public static void Initialize() => _ = _ui.Value;

    private static object StartUiThread()
    {
        var started = new TaskCompletionSource();

        var thread = new Thread(() =>
        {
            try
            {
                AppBuilder.Configure<WinGetXAvalonia.App>()
                    .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                    .SetupWithoutStarting();
                started.SetResult();
            }
            catch (Exception ex)
            {
                started.SetException(ex);
            }

            Dispatcher.UIThread.MainLoop(CancellationToken.None);
        })
        {
            IsBackground = true,
        };

        thread.Start();
        started.Task.GetAwaiter().GetResult();

        return new object();
    }
}
