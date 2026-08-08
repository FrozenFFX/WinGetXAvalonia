// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Notifications;

namespace WinGetXAvalonia.Services.Core;

public interface IErrorHandlingService
{
    UserFriendlyErrorInfo HandlePackageOperationError(Exception ex, string operation, string? packageId = null);
    UserFriendlyErrorInfo HandleWinGetError(Exception ex, string operation, string? packageId = null);
    UserFriendlyErrorInfo HandleSourceError(Exception ex, string? source = null);
    UserFriendlyErrorInfo HandleNetworkError(Exception ex, string? operation = null);
    UserFriendlyErrorInfo HandleDependencyError(Exception ex, string? packageId = null);
    UserFriendlyErrorInfo HandlePermissionError(Exception ex, string? operation = null);
    UserFriendlyErrorInfo HandleTimeoutError(Exception ex, string? operation = null);
}

public class ErrorHandlingService : IErrorHandlingService
{
    private readonly LoggerService _logger;
    private readonly IHistoryService _history;
    private readonly NotificationService _notifications;

    public ErrorHandlingService(
        LoggerService logger,
        IHistoryService history,
        NotificationService notifications)
    {
        _logger = logger;
        _history = history;
        _notifications = notifications;
    }

    public UserFriendlyErrorInfo HandlePackageOperationError(Exception ex, string operation, string? packageId = null)
    {
        var packageName = packageId ?? "this package";
        
        if (ex is InvalidOperationException && ex.Message.Contains("not found"))
        {
            var errorInfo = new UserFriendlyErrorInfo
            {
                ErrorCode = ErrorCode.PackageNotFound,
                PackageId = packageId ?? "",
                UserMessage = $"Package '{packageName}' was not found on the configured source.",
                ActionableAdvice = "Please verify the package name and ensure the source is updated."
            };
            _LogAndNotifyError(errorInfo, ex, $"Package operation '{operation}' failed for '{packageName}': {ex.Message}");
            return errorInfo;
        }

        if (ex is InvalidOperationException && ex.Message.Contains("already installed"))
        {
            var errorInfo = new UserFriendlyErrorInfo
            {
                ErrorCode = ErrorCode.PackageAlreadyInstalled,
                PackageId = packageId ?? "",
                UserMessage = $"Package '{packageName}' is already installed.",
                ActionableAdvice = "Use 'Updates' to check for newer versions or 'Remove' to uninstall."
            };
            _LogAndNotifyError(errorInfo, ex, $"Package operation '{operation}' failed: Package already installed");
            return errorInfo;
        }

        if (ex is InvalidOperationException && ex.Message.Contains("version conflict"))
        {
            var errorInfo = new UserFriendlyErrorInfo
            {
                ErrorCode = ErrorCode.VersionConflict,
                PackageId = packageId ?? "",
                UserMessage = $"Version conflict for '{packageName}'.",
                ActionableAdvice = "Try specifying a different version with 'packageId::version' syntax or force reinstall."
            };
            _LogAndNotifyError(errorInfo, ex, $"Package operation '{operation}' failed for '{packageName}': Version conflict");
            return errorInfo;
        }

        var genericErrorInfo = new UserFriendlyErrorInfo
        {
            ErrorCode = ErrorCode.OperationFailed,
            PackageId = packageId ?? "",
            UserMessage = $"Unable to {operation} '{packageName}'.",
            ActionableAdvice = "Please check your connection and try again."
        };
        _LogAndNotifyError(genericErrorInfo, ex, $"Package operation '{operation}' failed for '{packageName}': {ex.Message}");
        return genericErrorInfo;
    }

    public UserFriendlyErrorInfo HandleWinGetError(Exception ex, string operation, string? packageId = null)
    {
        var packageName = packageId ?? "this package";

        if (ex.Message.Contains("source", StringComparison.OrdinalIgnoreCase))
        {
            if (ex.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase))
            {
                var errorInfo = new UserFriendlyErrorInfo
                {
                    ErrorCode = ErrorCode.SourceUnavailable,
                    PackageId = packageId ?? "",
                    UserMessage = $"Source error for '{packageName}'.",
                    ActionableAdvice = $"Please try again or use a different source. If the problem persists, report to the source maintainer."
                };
                _LogAndNotifyError(errorInfo, ex, $"WinGet source error for '{packageName}': {ex.Message}");
                return errorInfo;
            }

            var sourceErrorInfo = new UserFriendlyErrorInfo
            {
                ErrorCode = ErrorCode.SourceError,
                PackageId = packageId ?? "",
                UserMessage = $"Source configuration error.",
                ActionableAdvice = $"Check your winget configuration and try refreshing sources."
            };
            _LogAndNotifyError(sourceErrorInfo, ex, $"WinGet source error for '{packageName}': {ex.Message}");
            return sourceErrorInfo;
        }

        if (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase))
        {
            var errorInfo = new UserFriendlyErrorInfo
            {
                ErrorCode = ErrorCode.Timeout,
                PackageId = packageId ?? "",
                UserMessage = $"Operation timed out for '{packageName}'.",
                ActionableAdvice = $"Check your internet connection and try again. Large packages may need more time."
            };
            _LogAndNotifyError(errorInfo, ex, $"WinGet timeout for '{packageName}': {ex.Message}");
            return errorInfo;
        }

        if (ex.Message.Contains("slow", StringComparison.OrdinalIgnoreCase))
        {
            var errorInfo = new UserFriendlyErrorInfo
            {
                ErrorCode = ErrorCode.SlowConnection,
                PackageId = packageId ?? "",
                UserMessage = $"Operation is taking longer than expected for '{packageName}'.",
                ActionableAdvice = $"This may be due to slow network or server. Please wait or try again later."
            };
            _LogAndNotifyError(errorInfo, ex, $"WinGet slow connection for '{packageName}': {ex.Message}");
            return errorInfo;
        }

        var winGetErrorInfo = new UserFriendlyErrorInfo
        {
            ErrorCode = ErrorCode.WinGetError,
            PackageId = packageId ?? "",
            UserMessage = $"Unable to {operation} '{packageName}'.",
            ActionableAdvice = "Check your connection and try again. If the problem persists, try a different source."
        };
        _LogAndNotifyError(winGetErrorInfo, ex, $"WinGet error for '{packageName}': {ex.Message}");
        return winGetErrorInfo;
    }

    public UserFriendlyErrorInfo HandleSourceError(Exception ex, string? source = null)
    {
        var errorInfo = new UserFriendlyErrorInfo
        {
            ErrorCode = ErrorCode.SourceUnavailable,
            UserMessage = $"Source '{source ?? "the Microsoft source"}' is currently unavailable.",
            ActionableAdvice = $"Please try again in a few minutes or configure a different source in Settings."
        };
        _LogAndNotifyError(errorInfo, ex, $"Source error: {ex.Message}");
        return errorInfo;
    }

    public UserFriendlyErrorInfo HandleNetworkError(Exception ex, string? operation = null)
    {
        var errorInfo = new UserFriendlyErrorInfo
        {
            ErrorCode = ErrorCode.NetworkError,
            UserMessage = $"Network error occurred while {operation ?? "performing operation"}.",
            ActionableAdvice = "Check your internet connection and try again. If you have a proxy, verify it's working."
        };
        _LogAndNotifyError(errorInfo, ex, $"Network error during {operation}: {ex.Message}");
        return errorInfo;
    }

    public UserFriendlyErrorInfo HandleDependencyError(Exception ex, string? packageId = null)
    {
        var packageName = packageId ?? "this package";
        var errorInfo = new UserFriendlyErrorInfo
        {
            ErrorCode = ErrorCode.DependencyError,
            PackageId = packageId ?? "",
            UserMessage = $"Dependency issue for '{packageName}'.",
            ActionableAdvice = "Install required dependencies separately or update to a version that fixes the issue."
        };
        _LogAndNotifyError(errorInfo, ex, $"Dependency error for '{packageName}': {ex.Message}");
        return errorInfo;
    }

    public UserFriendlyErrorInfo HandlePermissionError(Exception ex, string? operation = null)
    {
        var errorInfo = new UserFriendlyErrorInfo
        {
            ErrorCode = ErrorCode.PermissionError,
            UserMessage = $"Permission denied while {operation}.",
            ActionableAdvice = "Run as administrator or check your user permissions."
        };
        _LogAndNotifyError(errorInfo, ex, $"Permission error during {operation}: {ex.Message}");
        return errorInfo;
    }

    public UserFriendlyErrorInfo HandleTimeoutError(Exception ex, string? operation = null)
    {
        var errorInfo = new UserFriendlyErrorInfo
        {
            ErrorCode = ErrorCode.Timeout,
            UserMessage = $"Operation timed out while {operation}.",
            ActionableAdvice = "Check your network connection and try again. Large packages may need more time."
        };
        _LogAndNotifyError(errorInfo, ex, $"Timeout during {operation}: {ex.Message}");
        return errorInfo;
    }

    private void _LogAndNotifyError(UserFriendlyErrorInfo errorInfo, Exception ex, string logMessage)
    {
        _logger.Error(ex, logMessage);
        _history.AddAsync(new HistoryEntry
        {
            Timestamp = DateTime.UtcNow,
            Operation = "Error",
            PackageId = errorInfo.PackageId,
            Success = false,
            Message = errorInfo.UserMessage
        }).FireAndForget(_logger, "Error history entry");
        _notifications.Notify(
            NotificationType.Error,
            $"{errorInfo.ErrorCode.GetErrorDisplayName()}",
            $"{errorInfo.UserMessage}\n\n{errorInfo.ActionableAdvice}");
    }
}

public class UserFriendlyErrorInfo
{
    public ErrorCode ErrorCode { get; set; }
    public string UserMessage { get; set; } = "";
    public string ActionableAdvice { get; set; } = "";
    public string PackageId { get; set; } = "";
    public Exception? OriginalException { get; set; }
}

public enum ErrorCode
{
    None,
    PackageNotFound,
    PackageAlreadyInstalled,
    VersionConflict,
    SourceUnavailable,
    SourceError,
    DependencyError,
    NetworkError,
    Timeout,
    SlowConnection,
    PermissionError,
    OperationFailed,
    WinGetError
}

public static class ErrorCodeExtensions
{
    public static string GetErrorDisplayName(this ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.PackageNotFound => "Package Not Found",
            ErrorCode.PackageAlreadyInstalled => "Already Installed",
            ErrorCode.VersionConflict => "Version Conflict",
            ErrorCode.SourceUnavailable => "Source Unavailable",
            ErrorCode.SourceError => "Source Error",
            ErrorCode.DependencyError => "Dependency Error",
            ErrorCode.NetworkError => "Network Error",
            ErrorCode.Timeout => "Timeout",
            ErrorCode.SlowConnection => "Slow Connection",
            ErrorCode.PermissionError => "Permission Denied",
            ErrorCode.OperationFailed => "Operation Failed",
            ErrorCode.WinGetError => "WinGet Error",
            _ => "Error"
        };
    }

    public static string GetUserMessage(this ErrorCode errorCode, string? packageName = null)
    {
        return errorCode switch
        {
            ErrorCode.PackageNotFound => $"Package '{packageName}' was not found.",
            ErrorCode.PackageAlreadyInstalled => $"Package '{packageName}' is already installed.",
            ErrorCode.VersionConflict => $"Version conflict for '{packageName}'.",
            ErrorCode.SourceUnavailable => $"Source is currently unavailable.",
            ErrorCode.SourceError => $"There was a problem with the source.",
            ErrorCode.DependencyError => $"There are dependency issues with '{packageName}'.",
            ErrorCode.NetworkError => "Network connection error.",
            ErrorCode.Timeout => "The operation timed out.",
            ErrorCode.SlowConnection => "Operation is taking longer than expected.",
            ErrorCode.PermissionError => "Permission was denied.",
            ErrorCode.OperationFailed => "The operation failed.",
            ErrorCode.WinGetError => $"WinGet error occurred while operating on '{packageName}'.",
            _ => "An error occurred."
        };
    }
}
