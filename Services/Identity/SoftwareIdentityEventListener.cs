// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Services.Identity;

/// <summary>
/// Package yaşam döngüsü olaylarını dinler ve Software Identity
/// profillerini otomatik günceller. Modüller olaylarla konuşur.
/// </summary>
public sealed class SoftwareIdentityEventListener : IDisposable
{
    private readonly IIdentityService _identity;
    private readonly LoggerService _logger;
    private readonly IDisposable _subscription;

    public SoftwareIdentityEventListener(
        IIdentityService identity,
        IEventBus bus,
        LoggerService logger)
    {
        _identity = identity;
        _logger = logger;
        _subscription = bus.Subscribe<PackageChangedEvent>(OnPackageChanged);
    }

    private void OnPackageChanged(PackageChangedEvent e)
    {
        _ = SafeHandleAsync(e);
    }

    private async Task SafeHandleAsync(PackageChangedEvent e)
    {
        try
        {
            await HandleAsync(e);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Identity event handling failed for '{e.PackageId}'");
        }
    }

    public async Task HandleAsync(PackageChangedEvent e)
    {
        switch (e.Operation)
        {
            case PackageOperation.Installed:
                await _identity.RecordInstallAsync(new SoftwareIdentity
                {
                    Name = e.PackageId,
                    Source = "winget"
                });
                break;

            case PackageOperation.Updated:
                var forUpdate = await _identity.GetByNameAsync(e.PackageId);
                if (forUpdate is not null)
                    await _identity.RecordUpdateAsync(forUpdate.Id);
                break;

            case PackageOperation.Removed:
                var forRemoval = await _identity.GetByNameAsync(e.PackageId);
                if (forRemoval is not null)
                    await _identity.DeleteAsync(forRemoval.Id);
                break;
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}
