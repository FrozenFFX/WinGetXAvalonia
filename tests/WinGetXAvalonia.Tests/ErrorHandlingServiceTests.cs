// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Notifications;

namespace WinGetXAvalonia.Tests;

public class ErrorHandlingServiceTests
{
    private static string CreateTempFile()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "history.json");
    }

    private static (ErrorHandlingService Service, HistoryService History, NotificationService Notifications) Create()
    {
        var history = new HistoryService(CreateTempFile());
        var notifications = new NotificationService();
        var service = new ErrorHandlingService(new LoggerService(), history, notifications);
        return (service, history, notifications);
    }

    private static async Task<List<HistoryEntry>> AwaitEntriesAsync(HistoryService history, int count)
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);

        while (DateTime.UtcNow < deadline)
        {
            var entries = (await history.GetAllAsync()).ToList();

            if (entries.Count >= count)
                return entries;

            await Task.Delay(10);
        }

        return (await history.GetAllAsync()).ToList();
    }

    [Fact]
    public void HandlePackageOperationError_NotFound_ReturnsPackageNotFound()
    {
        var (service, _, _) = Create();

        var result = service.HandlePackageOperationError(
            new InvalidOperationException("package was not found"),
            "install",
            "Git.Git");

        Assert.Equal(ErrorCode.PackageNotFound, result.ErrorCode);
        Assert.Equal("Git.Git", result.PackageId);
        Assert.Contains("Git.Git", result.UserMessage);
        Assert.NotEmpty(result.ActionableAdvice);
    }

    [Fact]
    public async Task HandlePackageOperationError_NotFound_RecordsPackageIdInHistory()
    {
        var (service, history, _) = Create();

        service.HandlePackageOperationError(
            new InvalidOperationException("package was not found"),
            "install",
            "Git.Git");

        var entries = await AwaitEntriesAsync(history, 1);

        var entry = Assert.Single(entries);
        Assert.Equal("Git.Git", entry.PackageId);
        Assert.False(entry.Success);
        Assert.Equal("Error", entry.Operation);
    }

    [Fact]
    public async Task HandlePackageOperationError_PublishesErrorNotification()
    {
        var (service, _, notifications) = Create();

        service.HandlePackageOperationError(
            new InvalidOperationException("package was not found"),
            "install",
            "Git.Git");

        Assert.NotEmpty(notifications.Notifications);
        Assert.Equal(NotificationType.Error, notifications.Notifications[0].Type);
        Assert.Contains("Package Not Found", notifications.Notifications[0].Title);
    }

    [Fact]
    public void HandlePackageOperationError_AlreadyInstalled_ReturnsSpecificCode()
    {
        var (service, _, _) = Create();

        var result = service.HandlePackageOperationError(
            new InvalidOperationException("already installed"),
            "install",
            "Git.Git");

        Assert.Equal(ErrorCode.PackageAlreadyInstalled, result.ErrorCode);
        Assert.Equal("Git.Git", result.PackageId);
    }

    [Fact]
    public void HandleWinGetError_SourceUnavailable_ReturnsSourceUnavailable()
    {
        var (service, _, _) = Create();

        var result = service.HandleWinGetError(
            new InvalidOperationException("source is unavailable"),
            "install",
            "Git.Git");

        Assert.Equal(ErrorCode.SourceUnavailable, result.ErrorCode);
        Assert.Equal("Git.Git", result.PackageId);
    }

    [Fact]
    public void HandleWinGetError_SourceError_ReturnsSourceError()
    {
        var (service, _, _) = Create();

        var result = service.HandleWinGetError(
            new InvalidOperationException("configuration of source failed"),
            "install",
            "Git.Git");

        Assert.Equal(ErrorCode.SourceError, result.ErrorCode);
        Assert.Equal("Git.Git", result.PackageId);
    }

    [Fact]
    public void HandleWinGetError_Timeout_ReturnsTimeout()
    {
        var (service, _, _) = Create();

        var result = service.HandleWinGetError(
            new InvalidOperationException("operation timed out"),
            "update",
            "Git.Git");

        Assert.Equal(ErrorCode.Timeout, result.ErrorCode);
        Assert.Equal("Git.Git", result.PackageId);
    }

    [Fact]
    public async Task HandleDependencyError_RecordsPackageIdInHistory()
    {
        var (service, history, _) = Create();

        service.HandleDependencyError(new Exception("dependency failed"), "Git.Git");

        var entries = await AwaitEntriesAsync(history, 1);

        var entry = Assert.Single(entries);
        Assert.Equal("Git.Git", entry.PackageId);
        Assert.False(entry.Success);
        Assert.Contains("Git.Git", entry.Message);
    }
}
