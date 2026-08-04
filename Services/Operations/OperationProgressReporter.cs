// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Models.Operations;

namespace WinGetXAvalonia.Services.Operations;

public sealed class OperationProgressReporter : IOperationProgress
{
    private readonly OperationModel _operation;

    public OperationProgressReporter(OperationModel operation)
    {
        _operation = operation;
    }

    public void Report(int percentage, string message)
    {
        _operation.Progress = percentage;
        _operation.Subtitle = message;
    }
}
