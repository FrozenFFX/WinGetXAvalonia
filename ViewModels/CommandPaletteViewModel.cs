// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Collections;
using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Core.Domain.Palette;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class CommandPaletteViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;
    private readonly IPackageProvider _provider;
    private readonly PackageSearchCache _cache;
    private readonly IOperationService _operations;
    private readonly IHistoryService _history;
    private readonly LoggerService _logger;
    private readonly NotificationService _notifications;

    private CancellationTokenSource? _searchCts;
    private IReadOnlyList<CommandPaletteItem> _recentItems = new List<CommandPaletteItem>();

    public Func<Type, bool>? NavigateToPage { get; set; }

    public Action<string>? SearchPackages { get; set; }

    public TimeSpan DebounceDelay { get; set; } = TimeSpan.FromMilliseconds(180);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCommandMode))]
    [NotifyPropertyChangedFor(nameof(PlaceholderText))]
    [NotifyPropertyChangedFor(nameof(Prefix))]
    private string searchText = "";

    [ObservableProperty]
    private bool isOpen;

    [ObservableProperty]
    private int selectedIndex;

    public ObservableRangeCollection<CommandPaletteItem> Results { get; } = new();

    public bool IsCommandMode => PaletteQueryParser.IsCommandMode(SearchText);

    public string Prefix => IsCommandMode ? ">" : "🔍";

    public string PlaceholderText =>
        IsCommandMode
            ? "Type a command..."
            : "Search packages or type '>' for commands...";

    public CommandPaletteViewModel(
        NavigationService navigation,
        IPackageProvider provider,
        PackageSearchCache cache,
        IOperationService operations,
        IHistoryService history,
        LoggerService logger,
        NotificationService notifications)
    {
        _navigation = navigation;
        _provider = provider;
        _cache = cache;
        _operations = operations;
        _history = history;
        _logger = logger;
        _notifications = notifications;
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();

        _searchCts = new CancellationTokenSource();

        RebuildAsync(_searchCts.Token).FireAndForget(_logger, "Command palette search");
    }

    [RelayCommand]
    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        SearchText = "";
        SelectedIndex = 0;

        LoadRecentsAsync().FireAndForget(_logger, "Command palette recent items");
        RebuildAsync().FireAndForget(_logger, "Command palette rebuild");
    }

    [RelayCommand]
    public void Close()
    {
        if (!IsOpen)
            return;

        _searchCts?.Cancel();

        IsOpen = false;
        Results.Clear();
    }

    [RelayCommand]
    private void MoveUp()
    {
        if (Results.Count == 0)
            return;

        int index = SelectedIndex;
        int guard = Results.Count;

        while (guard-- > 0)
        {
            index = index <= 0 ? Results.Count - 1 : index - 1;

            if (!Results[index].IsHeader)
            {
                SelectedIndex = index;
                return;
            }
        }
    }

    [RelayCommand]
    private void MoveDown()
    {
        if (Results.Count == 0)
            return;

        int index = SelectedIndex;
        int guard = Results.Count;

        while (guard-- > 0)
        {
            index = index >= Results.Count - 1 ? 0 : index + 1;

            if (!Results[index].IsHeader)
            {
                SelectedIndex = index;
                return;
            }
        }
    }

    [RelayCommand]
    private async Task Execute(CommandPaletteItem? item)
    {
        item ??= SelectedIndex >= 0 && SelectedIndex < Results.Count
            ? Results[SelectedIndex]
            : null;

        if (item is null || item.IsHeader)
            return;

        switch (item.Action)
        {
            case CommandPaletteAction.StartInstall:
                ChainTo("install");
                return;

            case CommandPaletteAction.StartUpdate:
                ChainTo("update");
                return;

            case CommandPaletteAction.StartUninstall:
                ChainTo("uninstall");
                return;

            case CommandPaletteAction.OpenPage:
                await ExecuteOpenPageAsync(item);
                return;

            case CommandPaletteAction.InstallPackage:
            case CommandPaletteAction.UpdatePackage:
            case CommandPaletteAction.UninstallPackage:
                await RunPackageOperationAsync(item);
                return;

            case CommandPaletteAction.SearchPackages:
                if (!string.IsNullOrWhiteSpace(item.Parameter))
                {
                    SearchPackages?.Invoke(item.Parameter);
                    Close();
                }
                return;

            case CommandPaletteAction.RefreshCurrent:
                await RefreshCurrentPageAsync();
                Close();
                return;

            case CommandPaletteAction.UpgradeAll:
                await RunSafelyAsync(() => _operations.UpgradeAllAsync());
                Close();
                return;

            case CommandPaletteAction.ClearHistory:
                await RunSafelyAsync(() => _history.ClearAsync());
                Close();
                return;

            case CommandPaletteAction.NewWorkspace:
                NavigateToPage?.Invoke(typeof(WorkspacesViewModel));
                Close();
                return;

            case CommandPaletteAction.ExportHistory:
                NavigateToPage?.Invoke(typeof(HistoryViewModel));
                Close();
                return;
        }
    }

    public async Task RebuildAsync(CancellationToken token = default)
    {
        if (token != default)
        {
            try
            {
                await Task.Delay(DebounceDelay, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (token.IsCancellationRequested)
                return;
        }

        if (!IsOpen)
            return;

        var items = new List<CommandPaletteItem>();

        if (IsCommandMode)
        {
            items.Add(Header("Commands"));
            items.AddRange(MatchCommands(PaletteQueryParser.CommandText(SearchText)));
        }
        else
        {
            string query = SearchText.Trim();
            var (verb, argument) = PaletteQueryParser.Split(query);

            if (query.Length == 0)
            {
                items.Add(Header("Actions"));
                items.AddRange(ActionCommands());
                items.AddRange(OpenPageActions());

                items.Add(Header("Recent"));
                items.AddRange(_recentItems);
            }
            else if (Verbs.Contains(verb))
            {
                items.Add(Header("Command"));
                items.AddRange(MatchCommands(query));

                if (argument.Length > 0)
                {
                    items.Add(Header("Packages"));
                    await AddSearchResultsAsync(items, argument, token);
                }
            }
            else
            {
                items.Add(Header("Quick"));
                items.AddRange(QuickCommands(query));

                items.Add(Header("Packages"));
                await AddSearchResultsAsync(items, query, token);
            }
        }

        if (token.IsCancellationRequested || !IsOpen)
            return;

        Results.ReplaceAll(items);
        SelectedIndex = FirstSelectableIndex();
    }

    private void ChainTo(string verb)
    {
        SearchText = verb + " ";
        SelectedIndex = 0;
    }

    private Task ExecuteOpenPageAsync(CommandPaletteItem item)
    {
        if (item.PageType is null)
        {
            ChainTo("open");
            return Task.CompletedTask;
        }

        NavigateToPage?.Invoke(item.PageType);
        Close();

        return Task.CompletedTask;
    }

    private async Task RunPackageOperationAsync(CommandPaletteItem item)
    {
        string? id = item.Package?.Id ?? item.Parameter;

        if (string.IsNullOrWhiteSpace(id))
            return;

        if (item.Package is null && item.Parameter is not null)
        {
            var resolved = await ResolvePackageAsync(item.Parameter);

            if (resolved is null || string.IsNullOrWhiteSpace(resolved.Id))
            {
                _notifications.Notify(
                    NotificationType.Error,
                    "Package not found",
                    $"No package matched '{item.Parameter}'.");

                _logger.Error(
                    new InvalidOperationException($"No palette target for '{item.Parameter}'"),
                    "Command palette package not found");

                return;
            }

            id = resolved.Id;
        }

        Close();

        try
        {
            switch (item.Action)
            {
                case CommandPaletteAction.InstallPackage:
                    await _operations.InstallAsync(id);
                    break;

                case CommandPaletteAction.UpdatePackage:
                    await _operations.UpdateAsync(id);
                    break;

                case CommandPaletteAction.UninstallPackage:
                    await _operations.RemoveAsync(id);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Command palette operation failed for '{id}'");
        }
    }

    private async Task RefreshCurrentPageAsync()
    {
        var page = _navigation.CurrentPage;

        if (page is null)
            return;

        var property = page.GetType().GetProperty("RefreshCommand");

        if (property?.GetValue(page) is IAsyncRelayCommand command
            && command.CanExecute(null))
        {
            await command.ExecuteAsync(null);
        }
    }

    private async Task RunSafelyAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Command palette action failed");
        }
    }

    private async Task<PackageModel?> ResolvePackageAsync(string query)
    {
        try
        {
            var results = await GetOrSearchAsync(query);

            return SearchEngine.Rank(results, query).FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Command palette resolve failed for '{query}'");
            return null;
        }
    }

    private async Task AddSearchResultsAsync(
        List<CommandPaletteItem> items,
        string query,
        CancellationToken token)
    {
        try
        {
            var results = SearchEngine.Rank(
                await GetOrSearchAsync(query),
                query);

            token.ThrowIfCancellationRequested();

            foreach (var package in results.Take(8))
            {
                items.Add(new CommandPaletteItem
                {
                    Title = package.Name,
                    Subtitle = string.IsNullOrWhiteSpace(package.Id)
                        ? package.Source
                        : package.Id,
                    Icon = package.Icon,
                    Action = CommandPaletteAction.InstallPackage,
                    Package = package
                });
            }

            if (results.Count == 0)
            {
                items.Add(new CommandPaletteItem
                {
                    Title = $"No packages found for '{query}'",
                    IsHeader = true
                });
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Command palette search failed for '{query}'");
        }
    }

    private async Task<IReadOnlyList<PackageModel>> GetOrSearchAsync(string query)
    {
        if (_cache.TryGet(query, out var cached))
            return cached!;

        var results = await _provider.SearchAsync(query);

        _cache.Set(query, results);

        return results;
    }

    private async Task LoadRecentsAsync()
    {
        List<CommandPaletteItem> recents;

        try
        {
            var entries = await _history.GetAllAsync();

            recents = entries
                .Where(e => !string.IsNullOrWhiteSpace(e.PackageId))
                .GroupBy(e => e.PackageId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .Take(5)
                .Select(e => new CommandPaletteItem
                {
                    Title = string.IsNullOrWhiteSpace(e.PackageName)
                        ? e.PackageId
                        : e.PackageName,
                    Subtitle = $"Install {e.PackageId}",
                    Icon = "🕘",
                    Action = CommandPaletteAction.InstallPackage,
                    Parameter = e.PackageId
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load recent packages");
            return;
        }

        _recentItems = recents;

        if (IsOpen && string.IsNullOrWhiteSpace(SearchText))
            await RebuildAsync();
    }

    private List<CommandPaletteItem> MatchCommands(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return CommandDefinitions
                .Select(d => BuildCommandItem(d, ""))
                .Where(i => i is not null)
                .Cast<CommandPaletteItem>()
                .ToList();

        var matches = new List<CommandPaletteItem>();

        foreach (var definition in CommandDefinitions)
        {
            bool matched =
                definition.Phrase.StartsWith(text, StringComparison.OrdinalIgnoreCase)
                || text.StartsWith(definition.Phrase, StringComparison.OrdinalIgnoreCase);

            if (!matched)
                continue;

            string argument = "";

            if (definition.TakesArgument
                && text.StartsWith(definition.Phrase, StringComparison.OrdinalIgnoreCase))
            {
                argument = text[definition.Phrase.Length..].Trim();
            }

            var item = BuildCommandItem(definition, argument);

            if (item is not null)
                matches.Add(item);
        }

        return matches;
    }

    private CommandPaletteItem? BuildCommandItem(CommandDefinition definition, string argument)
    {
        switch (definition.Action)
        {
            case CommandPaletteAction.InstallPackage:
                return argument.Length > 0
                    ? Item($"Install {argument}", $"Search and install '{argument}'", "⬇", definition.Action, parameter: argument)
                    : Item("Install", "Type a package name to install", "⬇", definition.Action);

            case CommandPaletteAction.UpdatePackage:
                return argument.Length > 0
                    ? Item($"Update {argument}", $"Search and update '{argument}'", "🔄", definition.Action, parameter: argument)
                    : Item("Update", "Type a package name to update", "🔄", definition.Action);

            case CommandPaletteAction.UninstallPackage:
                return argument.Length > 0
                    ? Item($"Uninstall {argument}", $"Search and uninstall '{argument}'", "🗑", definition.Action, parameter: argument)
                    : Item("Uninstall", "Type a package name to uninstall", "🗑", definition.Action);

            case CommandPaletteAction.SearchPackages:
                return argument.Length > 0
                    ? Item($"Search for {argument}", $"Find '{argument}' in Discover", "🔍", definition.Action, parameter: argument)
                    : Item("Search", "Search packages in Discover", "🔍", definition.Action);

            case CommandPaletteAction.OpenPage:
                return BuildOpenPageItem(definition.PageType, argument);

            case CommandPaletteAction.RefreshCurrent:
                return Item("Refresh", "Refresh the current page", "🔄", definition.Action);

            case CommandPaletteAction.UpgradeAll:
                return Item("Upgrade all packages", "Install every available update", "⬆", definition.Action);

            case CommandPaletteAction.ClearHistory:
                return Item("Clear history", "Remove all operation history", "🧹", definition.Action);

            case CommandPaletteAction.NewWorkspace:
                return Item("New workspace", "Create a new workspace", "📄", definition.Action);

            case CommandPaletteAction.ExportHistory:
                return Item("Export history", "Export operation history", "📤", definition.Action);

            default:
                return null;
        }
    }

    private CommandPaletteItem BuildOpenPageItem(Type? pageType, string argument)
    {
        if (pageType is not null)
        {
            var entry = PageEntries.FirstOrDefault(p => p.Page == pageType);

            return Item(
                $"Open {entry.Name}",
                "Navigate to page",
                entry.Icon,
                CommandPaletteAction.OpenPage,
                pageType: pageType);
        }

        if (string.IsNullOrWhiteSpace(argument))
        {
            return Item(
                "Open a page",
                "Dashboard, packages, updates, history, workspaces, studio, settings...",
                "🧭",
                CommandPaletteAction.OpenPage);
        }

        return Item(
            $"Open \"{argument}\"",
            "Unknown page",
            "🧭",
            CommandPaletteAction.OpenPage,
            parameter: argument);
    }

    private IEnumerable<CommandPaletteItem> ActionCommands()
    {
        yield return Item("Install", "Install a package by name", "⬇", CommandPaletteAction.StartInstall);
        yield return Item("Update", "Update a package by name", "🔄", CommandPaletteAction.StartUpdate);
        yield return Item("Uninstall", "Remove a package by name", "🗑", CommandPaletteAction.StartUninstall);
    }

    private IEnumerable<CommandPaletteItem> OpenPageActions()
    {
        var pages = new[]
        {
            typeof(HistoryViewModel),
            typeof(SettingsViewModel),
            typeof(StudioViewModel),
            typeof(WorkspacesViewModel)
        };

        foreach (var page in pages)
        {
            var entry = PageEntries.First(p => p.Page == page);

            yield return Item(
                $"Open {entry.Name}",
                "Navigate to page",
                entry.Icon,
                CommandPaletteAction.OpenPage,
                pageType: page);
        }
    }

    private IEnumerable<CommandPaletteItem> QuickCommands(string query)
    {
        yield return Item($"Install {query}", $"Search and install '{query}'", "⬇", CommandPaletteAction.InstallPackage, parameter: query);
        yield return Item($"Update {query}", $"Search and update '{query}'", "🔄", CommandPaletteAction.UpdatePackage, parameter: query);
        yield return Item($"Uninstall {query}", $"Search and uninstall '{query}'", "🗑", CommandPaletteAction.UninstallPackage, parameter: query);
        yield return Item($"Search for {query}", $"Find '{query}' in Discover", "🔍", CommandPaletteAction.SearchPackages, parameter: query);
    }

    private int FirstSelectableIndex()
    {
        for (int i = 0; i < Results.Count; i++)
        {
            if (!Results[i].IsHeader)
                return i;
        }

        return 0;
    }

    private static CommandPaletteItem Header(string title)
        => new() { Title = title, IsHeader = true };

    private static CommandPaletteItem Item(
        string title,
        string subtitle,
        string icon,
        CommandPaletteAction action,
        string? parameter = null,
        Type? pageType = null)
        => new()
        {
            Title = title,
            Subtitle = subtitle,
            Icon = icon,
            Action = action,
            Parameter = parameter,
            PageType = pageType
        };

    private static readonly HashSet<string> Verbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "install", "update", "upgrade", "uninstall", "remove",
        "search", "find", "open",
        "refresh", "reload",
        "clear", "new", "export",
        "settings", "diagnostics"
    };

    private static readonly (Type Page, string Name, string Icon)[] PageEntries =
    {
        (typeof(DashboardViewModel), "Dashboard", "📊"),
        (typeof(InstalledViewModel), "Packages", "📦"),
        (typeof(UpdatesViewModel), "Updates", "🔄"),
        (typeof(BrowseViewModel), "Discover", "🔍"),
        (typeof(HistoryViewModel), "History", "🕘"),
        (typeof(WorkspacesViewModel), "Workspaces", "🗂"),
        (typeof(BlueprintsViewModel), "Blueprints", "📐"),
        (typeof(MarketplaceViewModel), "Marketplace", "🛍️"),
        (typeof(EnterpriseViewModel), "Enterprise", "🏢"),
        (typeof(SoftwareIdentityViewModel), "Identity", "🪪"),
        (typeof(AutomationViewModel), "Automation", "⚙"),
        (typeof(SecurityViewModel), "Security", "🛡"),
        (typeof(DiagnosticsViewModel), "Diagnostics", "🩺"),
        (typeof(SourceManagerViewModel), "Sources", "🌐"),
        (typeof(StudioViewModel), "Studio", "🎛"),
        (typeof(DeveloperViewModel), "Developer", "🧑‍💻"),
        (typeof(SettingsViewModel), "Settings", "⚙")
    };

    private static IReadOnlyList<CommandDefinition> CommandDefinitions { get; }
        = BuildCommandDefinitions();

    private static IReadOnlyList<CommandDefinition> BuildCommandDefinitions()
    {
        var list = new List<CommandDefinition>
        {
            new("install", CommandPaletteAction.InstallPackage, "⬇", true),
            new("update", CommandPaletteAction.UpdatePackage, "🔄", true),
            new("upgrade", CommandPaletteAction.UpdatePackage, "🔄", true),
            new("uninstall", CommandPaletteAction.UninstallPackage, "🗑", true),
            new("remove", CommandPaletteAction.UninstallPackage, "🗑", true),
            new("search", CommandPaletteAction.SearchPackages, "🔍", true),
            new("find", CommandPaletteAction.SearchPackages, "🔍", true),
            new("open", CommandPaletteAction.OpenPage, "🧭", true),
            new("refresh", CommandPaletteAction.RefreshCurrent, "🔄", false),
            new("reload", CommandPaletteAction.RefreshCurrent, "🔄", false),
            new("upgrade all", CommandPaletteAction.UpgradeAll, "⬆", false),
            new("clear history", CommandPaletteAction.ClearHistory, "🧹", false),
            new("new workspace", CommandPaletteAction.NewWorkspace, "📄", false),
            new("export history", CommandPaletteAction.ExportHistory, "📤", false)
        };

        foreach (var (page, name, icon) in PageEntries)
        {
            list.Add(new CommandDefinition(
                name.ToLowerInvariant(),
                CommandPaletteAction.OpenPage,
                icon,
                false,
                page));
        }

        list.Add(new CommandDefinition(
            "home", CommandPaletteAction.OpenPage, "🏠", false, typeof(DashboardViewModel)));
        list.Add(new CommandDefinition(
            "installed", CommandPaletteAction.OpenPage, "📦", false, typeof(InstalledViewModel)));
        list.Add(new CommandDefinition(
            "browse", CommandPaletteAction.OpenPage, "🔍", false, typeof(BrowseViewModel)));

        return list;
    }

    private sealed record CommandDefinition(
        string Phrase,
        CommandPaletteAction Action,
        string Icon,
        bool TakesArgument,
        Type? PageType = null);
}

