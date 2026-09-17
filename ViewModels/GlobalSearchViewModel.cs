// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;

namespace WinGetXAvalonia.ViewModels;

public partial class GlobalSearchViewModel : ViewModelBase
{
    private readonly IPackageProvider _provider;
    private readonly IHistoryService _history;
    private readonly ILauncherService _launcher;

    private string _searchText = "";

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                DebounceSearch().FireAndForget();
            }
        }
    }

    public ObservableCollection<SearchResult> Results { get; } = new();

    private readonly CancellationTokenSource _searchCts = new();

    public GlobalSearchViewModel(
        IPackageProvider provider,
        IHistoryService history,
        ILauncherService launcher)
    {
        _provider = provider;
        _history = history;
        _launcher = launcher;
    }

    public override Task InitializeAsync()
        => Task.CompletedTask;

    private async Task DebounceSearch()
    {
        var cts = new CancellationTokenSource();

        await Task.Delay(300, cts.Token).ContinueWith(_ =>
        {
            if (cts.IsCancellationRequested)
                return;

            SearchInternal().FireAndForget();
        });
    }

    [RelayCommand]
    private async Task SearchInternal()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Results.Clear();
            return;
        }

        Results.Clear();

        // Search packages - search by name/id from installed and updates
        try
        {
            var updates = await _provider.GetUpdatesAsync();
            var installed = await _provider.GetInstalledAsync();

            // Search installed packages
            foreach (var pkg in installed)
            {
                if (pkg.Name.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) ||
                    pkg.Id.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()))
                {
                    Results.Add(new SearchResult
                    {
                        Category = "Packages",
                        Title = pkg.Name,
                        Subtitle = pkg.Id,
                        Icon = "package",
                        Tag = pkg
                    });
                    if (Results.Count >= 10) break;
                }
            }

            // Search updates (available)
            if (Results.Count < 10)
            {
                foreach (var pkg in updates.Where(u => !string.IsNullOrWhiteSpace(u.Id)))
                {
                    if (pkg.Name.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) ||
                        pkg.Id.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()))
                    {
                        Results.Add(new SearchResult
                        {
                            Category = "Packages",
                            Title = pkg.Name,
                            Subtitle = pkg.Id,
                            Icon = "update",
                            Tag = pkg
                        });
                        if (Results.Count >= 10) break;
                    }
                }
            }
        }
        catch { }

        // Search history
        try
        {
            var historyEntries = await _history.GetAllAsync();

            if (historyEntries.Any())
            {
                foreach (var entry in historyEntries)
                {
                    if (entry.Operation?.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) == true ||
                        (entry.PackageId ?? "").ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) ||
                        (entry.Message ?? "").ToLowerInvariant().Contains(SearchText.ToLowerInvariant()))
                    {
                        Results.Add(new SearchResult
                        {
                            Category = "History",
                            Title = $"{entry.Operation} {entry.PackageId ?? ""}",
                            Subtitle = entry.Timestamp.ToString("ggg"),
                            Icon = "history",
                            Tag = entry
                        });
                        if (Results.Count >= 15) break;
                    }
                }
            }
        }
        catch { }
    }

    public void ClearSearch()
    {
        SearchText = "";
        Results.Clear();
    }
}

public class SearchResult
{
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Icon { get; set; } = "package";
    public object? Tag { get; set; }
}