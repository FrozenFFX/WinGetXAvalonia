using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class UpdatesViewModel : AsyncViewModel
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;
    private readonly IEventBus _eventBus;

    private bool _refreshing;

    [ObservableProperty]
    private UpdatePackage? selectedPackage;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string selectedSort = "Name";

    [ObservableProperty]
    private string selectedFilter = "All";

    public ObservableCollection<UpdatePackage> Updates { get; } = new();

    public int PackageCount => Updates.Count;

    public UpdatesViewModel(IPackageProvider provider, IOperationService operations, IEventBus eventBus)
    {
        _provider = provider;
        _operations = operations;
        _eventBus = eventBus;

        _eventBus.Subscribe<PackageChangedEvent>(e => _ = Refresh());
    }

    public Task InitializeAsync()
        => Refresh();

    [RelayCommand]
    async Task Refresh()
    {
        if (_refreshing)
            return;

        _refreshing = true;

        try
        {
            State = LoadState.Loading;
            ErrorMessage = null;

            Updates.Clear();

            foreach (var item in await _provider.GetUpdatesAsync())
                Updates.Add(item);

            State = Updates.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
        finally
        {
            _refreshing = false;
        }

        OnPropertyChanged(nameof(PackageCount));
    }

    [RelayCommand]
    async Task UpgradeAll()
    {
        await _operations.UpgradeAllAsync();
        await Refresh();
    }

    [RelayCommand]
    async Task Update(UpdatePackage? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        await _operations.UpdateAsync(package.Id);

        await Refresh();
    }

    [RelayCommand]
    void CopyId(UpdatePackage? package)
    {
    }

    [RelayCommand]
    void CopyName(UpdatePackage? package)
    {
    }

    [RelayCommand]
    void OpenHomepage(UpdatePackage? package)
    {
    }
}
