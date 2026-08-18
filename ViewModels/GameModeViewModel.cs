// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class GameModeViewModel : AsyncViewModel
{
    private readonly IPackageProvider? _provider;

    private readonly IOperationService? _operations;

    public ObservableCollection<WorkspacePackage> Packages { get; } = new();

    [ObservableProperty]
    private string searchText = "";

    public GameModeViewModel(
        IPackageProvider? provider = null,
        IOperationService? operations = null)
    {
        _provider = provider;
        _operations = operations;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    public override async Task InitializeAsync()
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            EnsureGamingPackages();

            var installed = await GetInstalledAsync();

            var installedIds = installed
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var package in Packages)
            {
                package.InstallState = installedIds.Contains(package.Id)
                    ? PackageInstallState.Installed
                    : PackageInstallState.NotInstalled;
            }

            State = Packages.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (System.Exception ex)
        {
            ErrorMessage = ex.Message;
            State = LoadState.Error;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await InitializeAsync();
    }

    [RelayCommand]
    private async Task InstallAsync(WorkspacePackage? package)
    {
        if (package is null || _operations is null)
            return;

        if (package.IsBusy || package.InstallState == PackageInstallState.Installed)
            return;

        package.InstallState = PackageInstallState.Installing;

        try
        {
            await _operations.InstallAsync(package.Id);
            package.InstallState = PackageInstallState.Installed;
        }
        catch (System.Exception)
        {
            package.InstallState = PackageInstallState.NotInstalled;
        }
    }

    [RelayCommand]
    private async Task InstallAllAsync()
    {
        if (_operations is null)
            return;

        var pending = Packages
            .Where(p => p.InstallState != PackageInstallState.Installed)
            .ToList();

        foreach (var package in pending)
            await InstallAsync(package);
    }

    public bool ShowInstallAll =>
        _operations is not null && Packages.Any(p => p.CanInstall);

    private void EnsureGamingPackages()
    {
        Packages.Clear();

        Packages.Add(new WorkspacePackage
        {
            Id = "Valve.Steam",
            Name = "Steam",
            Version = "latest",
            Source = "winget",
            Icon = "🎮"
        });

        Packages.Add(new WorkspacePackage
        {
            Id = "EpicGames.EpicGamesLauncher",
            Name = "Epic Games Launcher",
            Version = "latest",
            Source = "winget",
            Icon = "🎮"
        });

        Packages.Add(new WorkspacePackage
        {
            Id = "Discord.Discord",
            Name = "Discord",
            Version = "latest",
            Source = "winget",
            Icon = "💬"
        });

        Packages.Add(new WorkspacePackage
        {
            Id = "OBSProject.OBSStudio",
            Name = "OBS Studio",
            Version = "latest",
            Source = "winget",
            Icon = "📺"
        });
    }

    private async Task<string[]> GetInstalledAsync()
    {
        if (_provider is null)
            return System.Array.Empty<string>();

        var installed = await _provider.GetInstalledAsync();

        return installed
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .Select(x => x.Id)
            .ToArray();
    }

    private void ApplyFilter()
    {
    }
}
