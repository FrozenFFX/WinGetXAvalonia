// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Services.Core;

/// <summary>
/// WinGet CLI allows only one active process; parallel invocations can deadlock.
/// All winget process launches must pass through this gate.
/// </summary>
internal static class WingetExecutionGate
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public static async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
    {
        await Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        return Releaser.Instance;
    }

    private sealed class Releaser : IDisposable
    {
        public static readonly Releaser Instance = new();

        public void Dispose() => Gate.Release();
    }
}
