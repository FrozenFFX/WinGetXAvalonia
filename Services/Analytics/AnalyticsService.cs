// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Concurrent;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Services.Analytics;

public interface IAnalyticsService
{
    void Record(string feature, string action, bool success, double durationMs);

    AnalyticsSnapshot GetSnapshot();

    void Clear();
}

public sealed class FeatureKpi
{
    private long _count;
    private long _successCount;
    private double _totalDurationMs;

    public string Feature { get; init; } = string.Empty;

    public long Count => Interlocked.Read(ref _count);

    public long SuccessCount => Interlocked.Read(ref _successCount);

    public double TotalDurationMs => _totalDurationMs;

    public double SuccessRate => Count == 0 ? 0 : SuccessCount * 100.0 / Count;

    public double AverageDurationMs => Count == 0 ? 0 : TotalDurationMs / Count;

    internal void Add(bool success, double durationMs)
    {
        Interlocked.Increment(ref _count);

        if (success)
            Interlocked.Increment(ref _successCount);

        _totalDurationMs += durationMs;
    }
}

public sealed class AnalyticsSnapshot
{
    public long TotalOperations { get; init; }
    public long Successful { get; init; }
    public long Failed { get; init; }
    public double SuccessRate => TotalOperations == 0 ? 0 : Successful * 100.0 / TotalOperations;
    public double MinutesSaved { get; init; }
    public IReadOnlyDictionary<string, FeatureKpi> ByFeature { get; init; } = new Dictionary<string, FeatureKpi>();
}

/// <summary>
/// Her özelliğin KPI'ını (başarı oranı, süre, tahmini kazanılan dakika)
/// tek noktadan toplar. HATA 11/12/18: North Star metrik ve per-feature KPI.
/// </summary>
public sealed class AnalyticsService : IAnalyticsService
{
    private readonly ConcurrentDictionary<string, FeatureKpi> _byFeature = new(StringComparer.OrdinalIgnoreCase);
    private readonly LoggerService _logger;

    private long _total;
    private long _successful;
    private long _minutesTenths;

    public AnalyticsService(LoggerService logger)
    {
        _logger = logger;
    }

    public void Record(string feature, string action, bool success, double durationMs)
    {
        try
        {
            var kpi = _byFeature.GetOrAdd(
                feature,
                static name => new FeatureKpi { Feature = name });

            kpi.Add(success, durationMs);

            Interlocked.Increment(ref _total);

            if (success)
                Interlocked.Increment(ref _successful);

            if (success)
                Interlocked.Add(ref _minutesTenths, (long)Math.Round(ManualMinutesEquivalent(feature) * 10));

            _logger.Log($"[Analytics] {feature} | {action} | success={success} | {durationMs:0}ms");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to record analytics event: {feature} | {action}");
        }
    }

    public AnalyticsSnapshot GetSnapshot()
    {
        long total = Interlocked.Read(ref _total);
        long successful = Interlocked.Read(ref _successful);

        return new AnalyticsSnapshot
        {
            TotalOperations = total,
            Successful = successful,
            Failed = total - successful,
            MinutesSaved = Interlocked.Read(ref _minutesTenths) / 10.0,
            ByFeature = _byFeature
                .OrderByDescending(p => p.Value.Count)
                .ToDictionary(p => p.Key, p => p.Value)
        };
    }

    public void Clear()
    {
        _byFeature.Clear();
        Interlocked.Exchange(ref _total, 0);
        Interlocked.Exchange(ref _successful, 0);
        Interlocked.Exchange(ref _minutesTenths, 0);
    }

    /// <summary>
    /// Manuel yapılan işlem ile WinGetX arasındaki tahmini zaman farkı (dakika).
    /// North Star metrik "Minutes Saved" buradan hesaplanır.
    /// </summary>
    private static double ManualMinutesEquivalent(string feature)
    {
        if (feature.Contains("Install", StringComparison.OrdinalIgnoreCase))
            return 5.0;

        if (feature.Contains("Upgrade", StringComparison.OrdinalIgnoreCase))
            return 3.0;

        if (feature.Contains("Update", StringComparison.OrdinalIgnoreCase))
            return 2.0;

        if (feature.Contains("Remove", StringComparison.OrdinalIgnoreCase))
            return 2.0;

        if (feature.Contains("Repair", StringComparison.OrdinalIgnoreCase))
            return 5.0;

        return 1.0;
    }
}
