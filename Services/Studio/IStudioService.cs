// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Studio;

public interface IStudioService
{
    Task<IReadOnlyList<StudioTool>> GetToolsAsync();
}
