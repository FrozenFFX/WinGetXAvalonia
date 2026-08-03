// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
namespace WinGetXAvalonia.Interfaces;

// kaneki
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
// kaneki
