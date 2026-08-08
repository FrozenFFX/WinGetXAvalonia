// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Blueprints;

public class BlueprintStore : IBlueprintStore
{
    private readonly string _folder;

    public BlueprintStore()
        : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WinGetX",
                "Blueprints"))
    {
    }

    public BlueprintStore(string folder)
    {
        _folder = folder;
        Directory.CreateDirectory(folder);
    }

    public Task<IReadOnlyList<BlueprintModel>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = new List<BlueprintModel>();

        foreach (var file in Directory.GetFiles(_folder, "*.blueprint.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var blueprint = JsonSerializer.Deserialize<BlueprintModel>(
                    File.ReadAllText(file));

                if (blueprint is not null)
                    result.Add(blueprint);
            }
            catch (JsonException)
            {
                // bozuk dosya tüm listeyi bozmasın
            }
        }

        return Task.FromResult<IReadOnlyList<BlueprintModel>>(
            result.OrderBy(x => x.Name).ToList());
    }

    public Task SaveAsync(
        BlueprintModel blueprint,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = GetPath(blueprint.Id);

        File.WriteAllText(
            path,
            JsonSerializer.Serialize(blueprint, WriteOptions));

        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = GetPath(id);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<BlueprintModel>> ImportJsonAsync(
        string json,
        CancellationToken cancellationToken = default)
    {
        var imported = DeserializeImports(json);

        foreach (var blueprint in imported)
        {
            blueprint.Id = Guid.NewGuid();

            await SaveAsync(blueprint, cancellationToken);
        }

        return imported;
    }

    private static IReadOnlyList<BlueprintModel> DeserializeImports(string json)
    {
        try
        {
            var single = JsonSerializer.Deserialize<BlueprintModel>(json);

            if (single is not null)
                return new[] { single };
        }
        catch (JsonException)
        {
            // tek nesne değilse liste olarak dene
        }

        return JsonSerializer.Deserialize<List<BlueprintModel>>(json)
            ?? throw new InvalidDataException("Invalid blueprint file.");
    }

    private string GetPath(Guid id)
        => Path.Combine(_folder, $"{id}.blueprint.json");

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };
}
