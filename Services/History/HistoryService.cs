// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.History;

public sealed class HistoryService : IHistoryService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public HistoryService(string? filePath = null)
    {
        _filePath = filePath ?? DefaultPath();
    }

    private static string DefaultPath()
    {
        string appData =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrEmpty(appData))
            appData = Path.GetTempPath();

        return Path.Combine(appData, "WinGetX", "history.json");
    }

    public async Task AddAsync(HistoryEntry entry)
    {
        await _gate.WaitAsync();
        try
        {
            entry.Timestamp = DateTime.Now;

            var entries = await LoadEntriesAsync();
            entries.Add(entry);

            entries = entries
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            await SaveEntriesAsync(entries);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<HistoryEntry>> GetAllAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var entries = await LoadEntriesAsync();

            return entries
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearAsync()
    {
        await _gate.WaitAsync();
        try
        {
            await SaveEntriesAsync(new List<HistoryEntry>());
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<HistoryEntry>> LoadEntriesAsync()
    {
        if (!File.Exists(_filePath))
            return new List<HistoryEntry>();

        try
        {
            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<HistoryEntry>>(json, Options)
                ?? new List<HistoryEntry>();
        }
        catch (JsonException)
        {
            return new List<HistoryEntry>();
        }
    }

    private async Task SaveEntriesAsync(List<HistoryEntry> entries)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(entries, Options);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
