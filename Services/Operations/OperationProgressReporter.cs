// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Threading;
using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Models.Operations;

namespace WinGetXAvalonia.Services.Operations;

public sealed class OperationProgressReporter : IOperationProgress
{
    private readonly OperationModel _operation;
    private readonly SynchronizationContext? _context;

    public OperationProgressReporter(OperationModel operation)
    {
        _operation = operation;
        _context = SynchronizationContext.Current;
    }

    public void Report(int percentage, string message)
    {
        if (_context is not null && _context != SynchronizationContext.Current)
        {
            _context.Post(_ => Report(percentage, message), null);
            return;
        }

        _operation.Progress = percentage;
        _operation.Subtitle = message;
    }
}
