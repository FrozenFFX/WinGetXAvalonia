// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.Domain.Packages;

public sealed class PackageVersionComparer : IComparer<string>
{
    public static readonly PackageVersionComparer Instance = new();

    public int Compare(string? left, string? right) => CompareVersions(left, right);

    public static int CompareVersions(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) && string.IsNullOrWhiteSpace(right))
            return 0;

        if (string.IsNullOrWhiteSpace(left))
            return -1;

        if (string.IsNullOrWhiteSpace(right))
            return 1;

        int[] a = Parse(left!);
        int[] b = Parse(right!);

        int length = Math.Max(a.Length, b.Length);

        for (int i = 0; i < length; i++)
        {
            int leftPart = i < a.Length ? a[i] : 0;
            int rightPart = i < b.Length ? b[i] : 0;

            if (leftPart != rightPart)
                return leftPart.CompareTo(rightPart);
        }

        return 0;
    }

    public static bool IsNewer(string? candidate, string? current)
        => CompareVersions(candidate, current) > 0;

    public static bool IsSameOrNewer(string? candidate, string? current)
        => CompareVersions(candidate, current) >= 0;

    public static bool IsOlder(string? candidate, string? current)
        => CompareVersions(candidate, current) < 0;

    private static int[] Parse(string version)
        => version
            .Split('.')
            .Select(LeadingNumber)
            .ToArray();

    private static int LeadingNumber(string segment)
    {
        int value = 0;

        foreach (char c in segment)
        {
            if (c < '0' || c > '9')
                break;

            value = (value * 10) + (c - '0');
        }

        return value;
    }
}
