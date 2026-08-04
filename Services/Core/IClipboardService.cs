// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Services.Core;

public interface IClipboardService
{
    Task CopyTextAsync(string text);
}
