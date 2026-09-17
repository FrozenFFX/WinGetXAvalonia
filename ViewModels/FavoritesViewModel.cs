// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.ViewModels;

public partial class FavoritesViewModel : AsyncViewModel, IDisposable
{
    private readonly FavoritesService _favorites;
    private readonly IPackageProvider _provider;
    private readonly IEventBus _eventBus;
    private IDisposable? _subscription;

    [ObservableProperty]
    private string searchText = "";

    public ObservableCollection<PackageModel> FavoritePackages { get; } = new();

    public int FavoriteCount => FavoritePackages.Count;

    public FavoritesViewModel(
        FavoritesService favorites,
        IPackageProvider provider,
        IEventBus eventBus)
    {
        _favorites = favorites;
        _provider = provider;
        _eventBus = eventBus;

        FavoritePackages.CollectionChanged += (_, _) =>
            OnPropertyChanged(nameof(FavoriteCount));

        _subscription =
            _eventBus.Subscribe<PackageChangedEvent>(e =>
                Dispatcher.UIThread.Post(() => _ = Refresh()));
    }

    public override async Task InitializeAsync()
    {
        await _favorites.Load();
        await Refresh();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            FavoritePackages.Clear();

            foreach (var pkg in _favorites.Favorites)
            {
                var model = new PackageModel
                {
                    Icon = IconProvider.Get(pkg.Name),
                    Name = pkg.Name,
                    Id = pkg.Id,
                    Version = pkg.Version,
                    Source = pkg.Source
                };

                FavoritePackages.Add(model);
            }

            State = FavoritePackages.Count == 0
                ? LoadState.Empty
                : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = $"Failed to load favorites: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RemoveFavorite(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        await _favorites.Remove(package.Id);
        await Refresh();
    }

    [RelayCommand]
    private async Task Install(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        try
        {
            await _provider.InstallAsync(package.Id);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to install {package.Name}: {ex.Message}";
        }
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
