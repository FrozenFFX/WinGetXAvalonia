// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Intelligence;

public sealed record PackageHealthResult
{
    public string Level { get; set; } = "Unknown"; // Healthy | AttentionNeeded | Problem | Unknown
    public string Reason { get; set; } = "";
    public string Details { get; set; } = "";
}

public sealed record PackageRiskResult
{
    public string Level { get; set; } = "Unknown"; // Low | Medium | High | Unknown
    public List<string> Reasons { get; set; } = new();
    public string Confidence { get; set; } = "";
}

public sealed record InstallCostResult
{
    public string DownloadSize { get; set; } = "Not available";
    public string DiskUsage { get; set; } = "Not available";
    public string Dependencies { get; set; } = "Not available";
    public string Architecture { get; set; } = "";
    public string Scope { get; set; } = "";
    public string RestartRequired { get; set; } = "Not available";
    public string EstimatedTime { get; set; } = "Not available";
}

public sealed class PackageIntelligenceService
{
    /// <summary>
    /// Evaluates package health based on available metadata.
    /// </summary>
    public PackageHealthResult EvaluateHealth(PackageModel package)
    {
        var result = new PackageHealthResult();

        try
        {
            var reasons = new List<string>();

            if (package == null)
            {
                result.Level = "Problem";
                result.Reason = "Package is null";
                return result;
            }

            // Missing version metadata
            if (string.IsNullOrWhiteSpace(package.Version))
                reasons.Add("Missing version metadata");

            // Missing source
            if (string.IsNullOrWhiteSpace(package.Source))
                reasons.Add("Missing source");

            // Package name available
            if (string.IsNullOrWhiteSpace(package.Id))
                reasons.Add("Missing package ID");

            // Update availability - check HasUpdate if present
            if (package.HasUpdate)
                reasons.Add("Update available");

            if (reasons.Count == 0)
                result.Level = "Healthy";
            else if (reasons.Count == 1)
                result.Level = "Attention Needed";
            else
                result.Level = "Problem";

            result.Reason = string.Join("; ", reasons.Take(3));
        }
        catch
        {
            result.Level = "Unknown";
            result.Reason = "Evaluation error";
        }

        return result;
    }

    /// <summary>
    /// Evaluates package risk based on metadata availability.
    /// </summary>
    /// <remarks>
    /// Risk is NOT a malware detector. It indicates data completeness 
    /// and installation considerations only.
    /// </remarks>
    public PackageRiskResult EvaluateRisk(PackageModel package)
    {
        var result = new PackageRiskResult();

        try
        {
            var reasons = new List<string>();

            if (package == null)
            {
                result.Level = "High";
                result.Reasons.Add("Package is null");
                result.Confidence = "Poor";
                return result;
            }

            // Missing version
            if (string.IsNullOrWhiteSpace(package.Version))
                reasons.Add("Missing version");

            // Missing source
            if (string.IsNullOrWhiteSpace(package.Source))
                reasons.Add("Missing source");

            // Missing ID
            if (string.IsNullOrWhiteSpace(package.Id))
                reasons.Add("Missing package ID");

            // Determine level
            if (reasons.Count >= 3)
                result.Level = "High";
            else if (reasons.Count >= 1)
                result.Level = "Medium";
            else
                result.Level = "Low";

            result.Reasons = reasons;

            // Set confidence based on data availability
            if (result.Reasons.Count == 0)
                result.Confidence = "Good metadata available";
            else if (result.Reasons.Count <= 2)
                result.Confidence = $"Partial metadata ({result.Reasons.Count} concern(s))";
            else
                result.Confidence = "Limited metadata";
        }
        catch
        {
            result.Level = "Unknown";
            result.Reasons.Add("Evaluation error");
        }

        return result;
    }

    /// <summary>
    /// Estimates install cost based on available package metadata.
    /// </summary>
    public InstallCostResult EstimateInstallCost(PackageModel package)
    {
        var result = new InstallCostResult();

        try
        {
            if (package == null)
                return result;

            // Download size and disk usage - not in basic package model
            result.DownloadSize = "Not available from current data";
            result.DiskUsage = "Not available from current data";

            // Dependencies - basic model doesn't include this
            result.Dependencies = "Not available";

            // Architecture - not in basic model
            result.Architecture = "";

            // Scope - not in basic model
            result.Scope = "";

            // Restart requirement - cannot determine from basic package data alone
            result.RestartRequired = "Not available from current data";

            // Estimated time - not available from basic package data
            result.EstimatedTime = "Not available";
        }
        catch
        {
            // Keep defaults on error
        }

        return result;
    }

    /// <summary>
    /// Gets overall package intelligence summary.
    /// </summary>
    public PackageIntelligenceSummary GetSummary(PackageModel package)
    {
        return new PackageIntelligenceSummary
        {
            Health = EvaluateHealth(package),
            Risk = EvaluateRisk(package),
            InstallCost = EstimateInstallCost(package)
        };
    }
}

public sealed record PackageIntelligenceSummary
{
    public PackageHealthResult Health { get; set; } = new PackageHealthResult { Level = "Unknown" };
    public PackageRiskResult Risk { get; set; } = new PackageRiskResult { Level = "Unknown" };
    public InstallCostResult InstallCost { get; set; } = new InstallCostResult();
}