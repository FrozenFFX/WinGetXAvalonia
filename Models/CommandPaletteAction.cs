// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public enum CommandPaletteAction
{
    None,
    StartInstall,
    StartUpdate,
    StartUninstall,
    InstallPackage,
    UpdatePackage,
    UninstallPackage,
    SearchPackages,
    OpenPage,
    RefreshCurrent,
    UpgradeAll,
    ClearHistory,
    NewWorkspace,
    ExportHistory
}
