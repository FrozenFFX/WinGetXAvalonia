// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Identity;

namespace WinGetXAvalonia.Tests;

public class SoftwareIdentityServiceTests : IDisposable
{
    private readonly string _folder;

    public SoftwareIdentityServiceTests()
    {
        _folder = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
            // test temizliği ikincil; başarısızsa görmezden gel
        }
    }

    private SoftwareIdentityService CreateService()
        => new(_folder);

    private static SoftwareIdentity Sample(string name, string version = "1.0")
        => new()
        {
            Name = name,
            Publisher = "Test Publisher",
            Version = version,
            Source = "winget"
        };

    [Fact]
    public async Task EmptyFolder_ReturnsNoIdentities()
    {
        var service = CreateService();

        var all = await service.GetAllAsync();

        Assert.Empty(all);
    }

    [Fact]
    public async Task RecordInstall_CreatesIdentity()
    {
        var service = CreateService();

        var created = await service.RecordInstallAsync(Sample("Git"));

        var all = await service.GetAllAsync();

        var identity = Assert.Single(all);
        Assert.Equal(created.Id, identity.Id);
        Assert.Equal("Git", identity.Name);
        Assert.Equal(0, identity.UpdateCount);
        Assert.Equal(100, identity.HealthScore);
    }

    [Fact]
    public async Task RecordInstall_SameName_UpdatesExisting()
    {
        var service = CreateService();

        await service.RecordInstallAsync(Sample("Git", "2.50"));
        await service.RecordInstallAsync(Sample("Git", "2.51"));

        var all = await service.GetAllAsync();

        var identity = Assert.Single(all);
        Assert.Equal("2.51", identity.Version);
    }

    [Fact]
    public async Task GetByName_CaseInsensitive_FindsIdentity()
    {
        var service = CreateService();
        var created = await service.RecordInstallAsync(Sample("Git"));

        var found = await service.GetByNameAsync("git");

        Assert.NotNull(found);
        Assert.Equal(created.Id, found!.Id);
    }

    [Fact]
    public async Task GetByName_Unknown_ReturnsNull()
    {
        var service = CreateService();

        var found = await service.GetByNameAsync("Missing.App");

        Assert.Null(found);
    }

    [Fact]
    public async Task RecordUpdate_IncrementsUpdateCount()
    {
        var service = CreateService();

        var created = await service.RecordInstallAsync(Sample("Chrome"));

        await service.RecordUpdateAsync(created.Id);

        var identity = await service.GetAsync(created.Id);

        Assert.NotNull(identity);
        Assert.Equal(1, identity!.UpdateCount);
        Assert.NotNull(identity.LastUpdatedOn);
    }

    [Fact]
    public async Task RecordRollback_DecreasesHealth()
    {
        var service = CreateService();

        var created = await service.RecordInstallAsync(Sample("Python"));

        await service.RecordRollbackAsync(created.Id);

        var identity = await service.GetAsync(created.Id);

        Assert.NotNull(identity);
        Assert.Equal(1, identity!.RollbackCount);
        Assert.Equal(75, identity.HealthScore);
    }

    [Fact]
    public async Task RecordUpdate_UnknownId_Throws()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RecordUpdateAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Delete_RemovesIdentity()
    {
        var service = CreateService();

        var created = await service.RecordInstallAsync(Sample("Node.js"));

        await service.DeleteAsync(created.Id);

        var all = await service.GetAllAsync();

        Assert.Empty(all);
    }

    [Fact]
    public async Task GetAll_PersistsAcrossServiceInstances()
    {
        var service = CreateService();
        await service.RecordInstallAsync(Sample("VSCode"));

        var reloaded = new SoftwareIdentityService(_folder);

        var all = await reloaded.GetAllAsync();

        var identity = Assert.Single(all);
        Assert.Equal("VSCode", identity.Name);
    }

    [Fact]
    public async Task CorruptFile_IsIgnored()
    {
        var service = CreateService();
        await service.RecordInstallAsync(Sample("Git"));

        File.WriteAllText(
            Path.Combine(_folder, "garbage.identity.json"),
            "{ not valid json");

        var all = await service.GetAllAsync();

        var identity = Assert.Single(all);
        Assert.Equal("Git", identity.Name);
    }

    [Fact]
    public async Task Health_NeverExceedsBounds()
    {
        var service = CreateService();

        var created = await service.RecordInstallAsync(Sample("Fragile"));

        for (var i = 0; i < 20; i++)
            await service.RecordRollbackAsync(created.Id);

        var identity = await service.GetAsync(created.Id);

        Assert.NotNull(identity);
        Assert.InRange(identity!.HealthScore, 0, 100);
    }
}
