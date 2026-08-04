// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Interfaces;

public interface IWingetService
{
    Task<List<string>> GetInstalledApps();
    Task<List<string>> GetUpdates();
    Task<List<string>> Search(string query);
    Task Install(string id);
    Task Uninstall(string id);
    Task Upgrade(string id);
    Task UpgradeAll();
    Task<string> Version();
}
