// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Blueprints;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Marketplace;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Theme;
using WinGetXAvalonia.Services.Workspaces;

namespace WinGetXAvalonia.ViewModels;

public partial class MarketplaceViewModel : AsyncViewModel
{
    private readonly MarketplaceService _marketplace;
    private readonly IBlueprintStore _blueprintStore;
    private readonly IWorkspaceService _workspaces;
    private readonly ThemeManager _themeManager;
    private readonly IDialogService _dialogs;
    private readonly INotificationService _notifications;
    private readonly IHistoryService _history;
    private readonly LoggerService _logger;

    private readonly List<MarketplaceItem> _all = new();

    public ObservableCollection<MarketplaceTab> Tabs { get; } = new();

    public ObservableCollection<MarketplaceItem> Items { get; } = new();

    [ObservableProperty]
    private MarketplaceTab? selectedTab;

    [ObservableProperty]
    private MarketplaceItem? selectedItem;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResultSummary))]
    private int resultCount;

    public string ResultSummary => ResultCount switch
    {
        0 => "No items",
        1 => "1 item",
        var count => $"{count} items"
    };

    public bool HasNoItems => Items.Count == 0;

    public MarketplaceViewModel(
        MarketplaceService marketplace,
        IBlueprintStore blueprintStore,
        IWorkspaceService workspaces,
        ThemeManager themeManager,
        IDialogService dialogs,
        INotificationService notifications,
        IHistoryService history,
        LoggerService logger)
    {
        _marketplace = marketplace;
        _blueprintStore = blueprintStore;
        _workspaces = workspaces;
        _themeManager = themeManager;
        _dialogs = dialogs;
        _notifications = notifications;
        _history = history;
        _logger = logger;

        Items.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasNoItems));
            OnPropertyChanged(nameof(ResultCount));
        };

        Tabs.Add(new MarketplaceTab
        {
            Kind = MarketplaceKind.Blueprint,
            Title = "Blueprints",
            Icon = "🧩",
            Description = "Reusable installation templates"
        });

        Tabs.Add(new MarketplaceTab
        {
            Kind = MarketplaceKind.Workspace,
            Title = "Workspaces",
            Icon = "🗂",
            Description = "Curated package collections"
        });

        Tabs.Add(new MarketplaceTab
        {
            Kind = MarketplaceKind.Theme,
            Title = "Themes",
            Icon = "🎨",
            Description = "Accent color and theme presets"
        });

        SelectedTab = Tabs[0];
    }

    public override Task InitializeAsync()
        => LoadAsync();

    partial void OnSelectedTabChanged(MarketplaceTab? value)
        => ApplyFilter();

    partial void OnSearchTextChanged(string value)
        => ApplyFilter();

    [RelayCommand]
    public async Task LoadAsync()
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            _all.Clear();
            _all.AddRange(_marketplace.Catalog.Items);

            await RefreshInstalledAsync();

            ApplyFilter();

            State = Items.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Marketplace catalog load failed");
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
    }

    private async Task RefreshInstalledAsync()
    {
        var blueprintNames = (await _blueprintStore.GetAllAsync())
            .Select(b => b.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var workspaceNames = (await _workspaces.GetAllAsync())
            .Select(w => w.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var accentNames = _themeManager.AllAccents
            .Select(a => a.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var item in _all)
        {
            item.IsInstalled = item.Kind switch
            {
                MarketplaceKind.Blueprint => blueprintNames.Contains(item.Name),
                MarketplaceKind.Workspace => workspaceNames.Contains(item.Name),
                MarketplaceKind.Theme => accentNames.Contains(item.Name),
                _ => false
            };
        }
    }

    [RelayCommand]
    private void SelectTab(MarketplaceTab? tab)
    {
        if (tab is not null)
            SelectedTab = tab;
    }

    [RelayCommand]
    private void SelectItem(MarketplaceItem? item)
    {
        if (item is not null)
            SelectedItem = item;
    }

    [RelayCommand]
    private async Task InstallAsync(MarketplaceItem? item)
    {
        if (item is null || !item.CanInstall)
            return;

        item.IsInstalling = true;

        try
        {
            switch (item.Kind)
            {
                case MarketplaceKind.Blueprint:
                    await InstallBlueprintAsync(item);
                    break;

                case MarketplaceKind.Workspace:
                    await InstallWorkspaceAsync(item);
                    break;

                case MarketplaceKind.Theme:
                    await InstallThemeAsync(item);
                    break;

                default:
                    throw new NotSupportedException($"Unknown kind: {item.Kind}");
            }

            item.IsInstalled = true;
            item.IsInstalling = false;

            _notifications.ShowSuccess(
                "Installed",
                $"'{item.Name}' is now ready to use.");

            await _history.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.Now,
                Operation = "Marketplace Install",
                PackageName = item.Name,
                PackageId = item.Id,
                Success = true
            });
        }
        catch (Exception ex)
        {
            item.IsInstalling = false;

            _logger.Error(ex, $"Marketplace install failed for '{item.Name}'");

            await _history.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.Now,
                Operation = "Marketplace Install",
                PackageName = item.Name,
                PackageId = item.Id,
                Success = false,
                Message = ex.Message
            });

            _notifications.ShowError("Install Failed", ex.Message);
        }
    }

    private Task InstallBlueprintAsync(MarketplaceItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Payload))
            throw new InvalidDataException("Blueprint payload is empty.");

        return _blueprintStore.ImportJsonAsync(item.Payload);
    }

    private Task InstallWorkspaceAsync(MarketplaceItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Payload))
            throw new InvalidDataException("Workspace payload is empty.");

        return _workspaces.ImportJsonAsync(item.Payload);
    }

    private async Task InstallThemeAsync(MarketplaceItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Payload))
            throw new InvalidDataException("Theme payload is empty.");

        var preset = JsonSerializer.Deserialize<ThemePreset>(item.Payload)
            ?? throw new InvalidDataException("Invalid theme payload.");

        await _themeManager.InstallAccent(
            new AccentDefinition(preset.Name, preset.Accent));
    }

    private void ApplyFilter()
    {
        if (SelectedTab is not { } tab)
            return;

        IEnumerable<MarketplaceItem> filtered = _all
            .Where(i => i.Kind == tab.Kind);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(i =>
                i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || i.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || i.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || i.Tags.Any(t => t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        }

        var matches = filtered.ToList();

        if (SelectedItem is not null && !matches.Contains(SelectedItem))
            SelectedItem = null;

        Items.Clear();

        foreach (var item in matches)
            Items.Add(item);

        if (State == LoadState.Loaded || State == LoadState.Empty)
            State = Items.Count == 0 ? LoadState.Empty : LoadState.Loaded;
    }
}

