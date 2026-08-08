// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Identity;

public sealed class SoftwareIdentityService : IIdentityService
{
    private readonly string _folder;

    public SoftwareIdentityService()
        : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WinGetX",
                "Identities"))
    {
        SeedDefaultCatalog();
    }

    public SoftwareIdentityService(string folder)
    {
        _folder = folder;
        Directory.CreateDirectory(folder);
    }

    public Task<IReadOnlyList<SoftwareIdentity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = new List<SoftwareIdentity>();

        foreach (var file in Directory.GetFiles(_folder, "*.identity.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var identity = JsonSerializer.Deserialize<SoftwareIdentity>(
                    File.ReadAllText(file));

                if (identity is not null)
                    result.Add(identity);
            }
            catch (JsonException)
            {
                // bozuk dosya tüm listeyi bozmasın
            }
        }

        return Task.FromResult<IReadOnlyList<SoftwareIdentity>>(
            result.OrderBy(x => x.Name).ToList());
    }

    public async Task<SoftwareIdentity?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.FirstOrDefault(x => x.Id == id);
    }

    public async Task<SoftwareIdentity?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.FirstOrDefault(x =>
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public Task SaveAsync(
        SoftwareIdentity identity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        File.WriteAllText(
            GetPath(identity.Id),
            JsonSerializer.Serialize(identity, WriteOptions));

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

    public async Task<SoftwareIdentity> RecordInstallAsync(
        SoftwareIdentity identity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var all = await GetAllAsync(cancellationToken);

        var existing = all.FirstOrDefault(x =>
            string.Equals(x.Name, identity.Name, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            existing.Publisher = identity.Publisher;
            existing.Version = identity.Version;
            existing.Source = identity.Source;
            existing.Workspace = identity.Workspace;
            existing.Blueprint = identity.Blueprint;
            existing.Automation = identity.Automation;
            existing.TrustScore = identity.TrustScore;
            existing.CommunityScore = identity.CommunityScore;
            existing.RepairSupported = identity.RepairSupported;
            existing.LastUpdatedOn = DateTime.Now;

            RecomputeHealth(existing);

            await SaveAsync(existing, cancellationToken);

            return existing;
        }

        identity.InstalledOn = DateTime.Now;
        identity.UpdateCount = 0;
        identity.RollbackCount = 0;
        identity.HealthScore = 100;

        await SaveAsync(identity, cancellationToken);

        return identity;
    }

    public async Task RecordUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var identity = await GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Software identity not found.");

        identity.UpdateCount++;
        identity.LastUpdatedOn = DateTime.Now;

        RecomputeHealth(identity);

        await SaveAsync(identity, cancellationToken);
    }

    public async Task RecordRollbackAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var identity = await GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Software identity not found.");

        identity.RollbackCount++;

        RecomputeHealth(identity);

        await SaveAsync(identity, cancellationToken);
    }

    private void SeedDefaultCatalog()
    {
        if (Directory.GetFiles(_folder, "*.identity.json").Length > 0)
            return;

        var now = DateTime.Now;

        var catalog = new[]
        {
            Create("Google Chrome", "Google", "141.0", "winget",
                workspace: "Daily", trust: 100, community: 4.9,
                repair: true, deps: new[] { "Chromium" }),
            Create("Visual Studio", "Microsoft", "17.12", "winget",
                workspace: "Developer", blueprint: "AI Developer",
                automation: "Weekly Update", trust: 98, community: 4.8,
                repair: true, rollbacks: 1, deps: new[] { ".NET SDK", "VC Runtime", "Windows SDK" }),
            Create("Python", "Python Software Foundation", "3.14", "winget",
                workspace: "AI Environment", trust: 96, community: 4.8,
                repair: false, deps: new[] { "VC Runtime" }),
            Create("Git", "Git", "2.51.0", "winget",
                workspace: "Developer", blueprint: "AI Developer",
                automation: "Weekly Update", trust: 100, community: 4.9,
                repair: true, deps: new[] { "OpenSSH" }),
            Create("Visual Studio Code", "Microsoft", "1.112", "winget",
                workspace: "Developer", trust: 99, community: 4.7,
                repair: false, deps: new[] { "VSCode CLI" }),
            Create("Node.js", "OpenJS", "24.0", "winget",
                workspace: "Web Environment", trust: 90, community: 4.6,
                repair: false, deps: Array.Empty<string>())
        };

        foreach (var identity in catalog)
            SaveAsync(identity, CancellationToken.None).GetAwaiter().GetResult();
    }

    private static SoftwareIdentity Create(
        string name,
        string publisher,
        string version,
        string source,
        string? workspace = null,
        string? blueprint = null,
        string? automation = null,
        int trust = 90,
        double community = 4.5,
        bool repair = false,
        int rollbacks = 0,
        string[]? deps = null)
    {
        var installed = DateTime.Now.AddDays(-(name.Length * 9) - 30);

        var identity = new SoftwareIdentity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Publisher = publisher,
            Version = version,
            Source = source,
            Workspace = workspace,
            Blueprint = blueprint,
            Automation = automation,
            TrustScore = trust,
            CommunityScore = community,
            RepairSupported = repair,
            InstalledOn = installed,
            LastUpdatedOn = installed.AddDays(7)
        };

        foreach (var dep in deps ?? Array.Empty<string>())
            identity.Dependencies.Add(dep);

        identity.RollbackCount = rollbacks;
        RecomputeHealth(identity);

        return identity;
    }

    private static void RecomputeHealth(SoftwareIdentity identity)
    {
        var health = 100;

        health -= identity.RollbackCount * 10;
        health -= identity.LastUpdatedOn is null ? 15 : 0;

        identity.HealthScore = Math.Clamp(health, 0, 100);
    }

    private string GetPath(Guid id)
        => Path.Combine(_folder, $"{id}.identity.json");

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };
}
