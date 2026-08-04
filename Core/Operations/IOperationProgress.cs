// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.Operations;

public interface IOperationProgress
{
    void Report(int percentage, string message);
}
