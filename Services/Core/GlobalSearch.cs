// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.History;

namespace WinGetXAvalonia.Services.Core;

public interface IGlobalSearch
{
    Task<SearchResult> SearchAsync(string query, GlobalSearchOptions? options = null);
    Task<IReadOnlyList<string>> GetSuggestionAsync(string prefix, int maxResults = 10);
    void IndexPackage(PackageModel package);
    void RemoveIndexedPackage(string packageId);
    event EventHandler<string>? OnProgressChanged;
}

public class GlobalSearchOptions
{
    public bool IncludePackages { get; set; } = true;
    public bool IncludeHistory { get; set; } = false;
    public PackageOperationFilter? OperationFilter { get; set; }
    public bool PreferInstalledFirst { get; set; } = false;
    public bool FeaturedBoost { get; set; } = true;
}

public enum PackageOperationFilter
{
    None,
    Install,
    Update,
    Remove
}

public class SearchResult
{
    public IReadOnlyList<PackageModel> Packages { get; set; } = new List<PackageModel>();
    public IReadOnlyList<HistoryEntry> History { get; set; } = new List<HistoryEntry>();
    public string Query { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public SearchStatistics Statistics { get; set; } = new SearchStatistics();
}

public class SearchStatistics
{
    public int TotalResults { get; set; }
    public int PackageCount { get; set; }
    public int HistoryCount { get; set; }
    public double SearchTimeMs { get; set; }
}

public class GlobalSearch : IGlobalSearch
{
    private readonly IPackageProvider _provider;
    private readonly IHistoryService _history;
    private readonly LoggerService _logger;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private readonly Dictionary<string, PackageModel> _inMemoryIndex = new();

    public event EventHandler<string>? OnProgressChanged;

    public GlobalSearch(
        IPackageProvider provider,
        IHistoryService history,
        LoggerService logger)
    {
        _provider = provider;
        _history = history;
        _logger = logger;
    }

    public async Task<SearchResult> SearchAsync(string query, GlobalSearchOptions? options = null)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        options ??= new GlobalSearchOptions();

        var result = new SearchResult
        {
            Query = query
        };

        try
        {
            OnProgressChanged?.Invoke(this, $"Searching globally for '{query}'...");

            if (options.IncludePackages)
            {
                OnProgressChanged?.Invoke(this, "Searching packages...");
                var packages = await _SearchPackagesAsync(query, options);
                result.Packages = packages;

                OnProgressChanged?.Invoke(this, "Indexing packages for next search...");
                await _IndexPackagesAsync(packages);
            }

            if (options.IncludeHistory)
            {
                OnProgressChanged?.Invoke(this, "Searching history...");
                result.History = await _SearchHistoryAsync(query);
            }

            sw.Stop();

            result.Statistics = new SearchStatistics
            {
                TotalResults = result.Packages.Count + result.History.Count,
                PackageCount = result.Packages.Count,
                HistoryCount = result.History.Count,
                SearchTimeMs = sw.ElapsedMilliseconds
            };

            OnProgressChanged?.Invoke(this, $"Search completed in {sw.ElapsedMilliseconds}ms");

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Global search failed for query: {query}");
            throw;
        }
    }

    public async Task<IReadOnlyList<string>> GetSuggestionAsync(string prefix, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
            return new List<string>();

        var suggestions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            OnProgressChanged?.Invoke(this, $"Generating suggestions for '{prefix}'...");

            var packageNames = _inMemoryIndex.Values
                .Where(p => p.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Name)
                .Distinct()
                .Take(maxResults)
                .ToList();

            suggestions.UnionWith(packageNames);

            var historyEntries = await _history.GetAllAsync();
            var historySuggestions = historyEntries
                .Select(h => h.PackageId)
                .Where(id => id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Take(maxResults - packageNames.Count)
                .ToList();

            suggestions.UnionWith(historySuggestions);

            OnProgressChanged?.Invoke(this, $"Generated {suggestions.Count} suggestions");

            return suggestions.Take(maxResults).ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to generate suggestions for prefix: {prefix}");
            return new List<string>();
        }
    }

    public void IndexPackage(PackageModel package)
    {
        if (package == null || string.IsNullOrWhiteSpace(package.Id))
            return;

        try
        {
            _inMemoryIndex[package.Id] = package;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to index package: {package.Id}");
        }
    }

    public void RemoveIndexedPackage(string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId))
            return;

        try
        {
            _inMemoryIndex.Remove(packageId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to remove indexed package: {packageId}");
        }
    }

    private async Task<List<PackageModel>> _SearchPackagesAsync(string query, GlobalSearchOptions options)
    {
        var results = new List<PackageModel>();

        try
        {
            var packages = await _provider.SearchAsync(query);

            foreach (var package in packages)
            {
                var score = 0;

                if (string.Equals(package.Id, query, StringComparison.OrdinalIgnoreCase))
                {
                    score += 100;
                }
                else if (package.Id.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    score += 80;
                }
                else if (package.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    score += 70;
                }
                else if (package.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    score += 60;
                }

                if (options.OperationFilter.HasValue)
                {
                    switch (options.OperationFilter.Value)
                    {
                        case PackageOperationFilter.Install:
                            if (package.InstallState == PackageInstallState.Installed)
                                score -= 20;
                            break;
                        case PackageOperationFilter.Update:
                            if (!package.HasUpdate)
                                score -= 20;
                            break;
                        case PackageOperationFilter.Remove:
                            if (package.InstallState != PackageInstallState.Installed)
                                score -= 20;
                            break;
                    }
                }

                if (score > 0 || string.IsNullOrWhiteSpace(query))
                {
                    package.Score = score;
                    results.Add(package);
                }
            }

            if (options.PreferInstalledFirst)
            {
                results = results
                    .OrderByDescending(p => p.InstallState == PackageInstallState.Installed)
                    .ThenByDescending(p => p.Score)
                    .ToList();
            }
            else
            {
                results = results.OrderByDescending(p => p.Score).ToList();
            }

            if (options.FeaturedBoost)
            {
                results = results.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.Score).ToList();
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to search packages for query: {query}");
            return results;
        }
    }

    private async Task<List<HistoryEntry>> _SearchHistoryAsync(string query)
    {
        try
        {
            var allHistory = await _history.GetAllAsync();

            return allHistory
                .Where(h =>
                    h.PackageId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    h.Operation.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(h => h.Timestamp)
                .Take(50)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to search history for query: {query}");
            return new List<HistoryEntry>();
        }
    }

    private async Task _IndexPackagesAsync(IReadOnlyList<PackageModel> packages)
    {
        await _indexLock.WaitAsync();
        try
        {
            foreach (var package in packages)
            {
                if (package != null && !string.IsNullOrWhiteSpace(package.Id))
                {
                    _inMemoryIndex[package.Id] = package;
                }
            }
        }
        finally
        {
            _indexLock.Release();
        }
    }
}
