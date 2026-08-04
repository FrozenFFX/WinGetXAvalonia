// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.Domain.Package;

public sealed record PackageIdentity(string Id, string Source)
{
    public static readonly PackageIdentity None = new("", "");

    public bool IsEmpty => string.IsNullOrWhiteSpace(Id);

    public string DisplayName => IsEmpty
        ? ""
        : string.IsNullOrWhiteSpace(Source)
            ? Id
            : $"{Id} ({Source})";

    public bool Matches(string id, string source)
        => string.Equals(Id, id, StringComparison.OrdinalIgnoreCase)
           && string.Equals(Source, source, StringComparison.OrdinalIgnoreCase);

    public bool Matches(PackageIdentity other)
        => other is not null && Matches(other.Id, other.Source);
}
