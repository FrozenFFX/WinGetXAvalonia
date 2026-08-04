// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.Package;

public enum SourceTrustLevel
{
    High,
    Medium,
    Low,
    Unknown
}

public enum UpdateStatus
{
    UpToDate,
    UpdateAvailable,
    NotInstalled
}

public sealed record PackageHealth(
    int Score,
    SourceTrustLevel SourceTrust,
    UpdateStatus UpdateStatus,
    bool IsPublisherVerified,
    string SourceTrustLabel,
    string LatestVersionLabel,
    string UpdateFrequencyLabel)
{
    public static readonly PackageHealth None = new(
        Score: 0,
        SourceTrust: SourceTrustLevel.Low,
        UpdateStatus: UpdateStatus.NotInstalled,
        IsPublisherVerified: false,
        SourceTrustLabel: "—",
        LatestVersionLabel: "—",
        UpdateFrequencyLabel: "—");

    public string ScoreLabel => $"{Score}%";

    public string PublisherLabel => IsPublisherVerified ? "✔ Verified" : "Not verified";
}

public static class PackageHealthCalculator
{
    private const int PerfectScore = 100;
    private const int LowTrustPenalty = 18;
    private const int MediumTrustPenalty = 8;
    private const int UpdatePendingPenalty = 7;
    private const int PrereleasePenalty = 5;

    public static PackageHealth Calculate(PackageModel package)
    {
        if (package is null)
            throw new ArgumentNullException(nameof(package));

        if (package.InstallState == PackageInstallState.Installed)
            return Calculate(package.Source, package.Version, package.Available);

        return CalculateNotInstalled(package);
    }

    public static PackageHealth Calculate(string source, string version, string available)
    {
        int score = PerfectScore;

        var sourceTrust = EvaluateSourceTrust(source);
        score -= sourceTrust.Penalty;

        UpdateStatus updateStatus = string.IsNullOrWhiteSpace(available)
            ? UpdateStatus.UpToDate
            : UpdateStatus.UpdateAvailable;

        if (updateStatus == UpdateStatus.UpdateAvailable)
            score -= UpdatePendingPenalty;

        var frequency = EvaluateUpdateFrequency(version);
        score -= frequency.Penalty;

        bool verified = sourceTrust.Level == SourceTrustLevel.High;

        score = Math.Max(0, Math.Min(PerfectScore, score));

        return new PackageHealth(
            Score: score,
            SourceTrust: sourceTrust.Level,
            UpdateStatus: updateStatus,
            IsPublisherVerified: verified,
            SourceTrustLabel: sourceTrust.Label,
            LatestVersionLabel: LatestVersionLabel(updateStatus, available),
            UpdateFrequencyLabel: frequency.Label);
    }

    private static PackageHealth CalculateNotInstalled(PackageModel package)
    {
        int score = PerfectScore;

        var sourceTrust = EvaluateSourceTrust(package.Source);
        score -= sourceTrust.Penalty;

        var frequency = EvaluateUpdateFrequency(package.Version);
        score -= frequency.Penalty;

        bool verified = sourceTrust.Level == SourceTrustLevel.High;

        score = Math.Max(0, Math.Min(PerfectScore, score));

        return new PackageHealth(
            Score: score,
            SourceTrust: sourceTrust.Level,
            UpdateStatus: UpdateStatus.NotInstalled,
            IsPublisherVerified: verified,
            SourceTrustLabel: sourceTrust.Label,
            LatestVersionLabel: "Not installed",
            UpdateFrequencyLabel: frequency.Label);
    }

    private static (SourceTrustLevel Level, int Penalty, string Label) EvaluateSourceTrust(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return (SourceTrustLevel.Unknown, LowTrustPenalty, "Unknown");

        if (source.Contains("winget", StringComparison.OrdinalIgnoreCase)
            || source.Contains("msstore", StringComparison.OrdinalIgnoreCase))
            return (SourceTrustLevel.High, 0, "High");

        if (source.Contains("portable", StringComparison.OrdinalIgnoreCase))
            return (SourceTrustLevel.Medium, MediumTrustPenalty, "Medium");

        return (SourceTrustLevel.Low, LowTrustPenalty, "Low");
    }

    private static (int Penalty, string Label) EvaluateUpdateFrequency(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return (0, "Unknown");

        string lowered = version.ToLowerInvariant();

        if (lowered.StartsWith("0.")
            || lowered.Contains("beta")
            || lowered.Contains("alpha")
            || lowered.Contains("preview")
            || lowered.Contains("rc"))
            return (PrereleasePenalty, "Developing");

        return (0, "Good");
    }

    private static string LatestVersionLabel(UpdateStatus status, string available)
        => status switch
        {
            UpdateStatus.UpToDate => "✔ Latest",
            UpdateStatus.UpdateAvailable => $"Update available ({available})",
            _ => "Not installed"
        };
}
