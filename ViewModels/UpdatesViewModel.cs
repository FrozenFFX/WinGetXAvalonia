// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

// kaneki
public partial class UpdatesViewModel : ViewModelBase
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;

    public ObservableCollection<UpdatePackage> Updates { get; } = new();

    public UpdatesViewModel(IPackageProvider provider, IOperationService operations)
    {
        _provider = provider;
        _operations = operations;
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
        await _operations.UpgradeAllAsync();
        await Refresh();
    }
}
// kaneki
