// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Interfaces;

public interface IWingetService
{
    Task<WingetResult> GetInstalledApps();
    Task<WingetResult> GetUpdates();
    Task<WingetResult> Search(string query);
    Task<WingetResult> Install(string id);
    Task<WingetResult> Uninstall(string id);
    Task<WingetResult> Upgrade(string id);
    Task<WingetResult> UpgradeAll();
    Task<WingetResult> Version();
}
