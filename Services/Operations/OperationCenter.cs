// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Models.Operations;
using WinGetXAvalonia.Services.Analytics;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Services.Operations;

public class OperationCenter : IOperationCenter
{
    private readonly LoggerService? _logger;
    private readonly IAnalyticsService? _analytics;
    private readonly ConcurrentDictionary<Guid, OperationModel> _operations = new();

    public OperationCenter(
        LoggerService? logger = null,
        IAnalyticsService? analytics = null)
    {
        _logger = logger;
        _analytics = analytics;
    }

    public ObservableCollection<OperationModel> Operations { get; } = new();

    public OperationModel Start(string title, string subtitle)
    {
        var operation = new OperationModel
        {
            Title = title,
            Subtitle = subtitle,
            Status = OperationStatus.Running
        };

        _operations[operation.Id] = operation;
        Operations.Insert(0, operation);
        _logger?.Log($"Started operation: {title}");

        return operation;
    }

    public void ReportProgress(Guid id, double progress, string subtitle)
    {
        if (!_operations.TryGetValue(id, out var operation))
            return;

        operation!.Progress = progress;
        operation.Subtitle = subtitle;
    }

    public void Complete(Guid id)
    {
        if (!_operations.TryGetValue(id, out var operation))
            return;

        operation!.Status = OperationStatus.Completed;
        operation.Progress = 100;
        operation.StatusMessage = "Completed";
        operation.CompletedAt = DateTime.UtcNow;
        _logger?.Log($"Completed operation: {operation.Title}");
        _analytics?.Record(Feature(operation), "Completed", true, operation.Duration.TotalMilliseconds);
    }

    public void Fail(Guid id, string error)
    {
        if (!_operations.TryGetValue(id, out var operation))
            return;

        operation!.Status = OperationStatus.Failed;
        operation.StatusMessage = error;
        operation.Subtitle = error;
        operation.CompletedAt = DateTime.UtcNow;
        _logger?.Log($"Failed operation: {operation.Title}. Error: {error}");
        _analytics?.Record(Feature(operation), "Failed", false, operation.Duration.TotalMilliseconds);
    }

    public void Remove(Guid id)
    {
        if (_operations.TryRemove(id, out var operation))
        {
            Operations.Remove(operation!);
        }
    }

    public void ClearCompleted()
    {
        foreach (var operation in Operations
                     .Where(x => x.Status == OperationStatus.Completed)
                     .ToList())
        {
            _operations.TryRemove(operation.Id, out _);
            Operations.Remove(operation);
        }
    }

    private static string Feature(OperationModel operation)
    {
        string subtitle = string.IsNullOrWhiteSpace(operation.Subtitle)
            ? operation.Title
            : operation.Subtitle;

        return "Operation." + subtitle.TrimEnd('.', ' ').Trim();
    }
}
