using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class UpdatesViewModel : ViewModelBase
{
    private readonly IPackageProvider _provider;

    public ObservableCollection<UpdatePackage> Updates { get; } = new();

    public UpdatesViewModel(IPackageProvider provider)
    {
        _provider = provider;
        _ = Refresh();
    }

    [RelayCommand]
    async Task Refresh()
    {
        Updates.Clear();

        foreach (var item in await _provider.GetUpdatesAsync())
            Updates.Add(item);
    }

    [RelayCommand]
    async Task UpgradeAll()
    {
        foreach (var update in Updates.ToList())
            await _provider.UpgradeAsync(update.Id);

        await Refresh();
    }
}
