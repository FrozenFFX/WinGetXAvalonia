using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Services;

namespace WinGetXAvalonia.ViewModels;

public partial class UpdatesViewModel : ViewModelBase
{
    private readonly WingetService _winget;

    public ObservableCollection<string> Updates { get; } = new();

    public UpdatesViewModel(WingetService winget)
    {
        _winget = winget;
        _ = Refresh();
    }

    [RelayCommand]
    async Task Refresh()
    {
        Updates.Clear();

        foreach (var item in await _winget.GetUpdates())
            Updates.Add(item);
    }

    [RelayCommand]
    async Task UpgradeAll()
    {
        await _winget.UpgradeAll();
    }
}
