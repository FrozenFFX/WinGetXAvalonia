// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Services.Core;

public interface ILauncherService
{
    Task OpenUrlAsync(string url);

    Task OpenFolderAsync(string path);

    Task OpenFileAsync(string path);
}
