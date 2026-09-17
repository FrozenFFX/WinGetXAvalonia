// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.Domain.Package;

public sealed record PackagePublisher(string Name, bool IsVerified)
{
    public static readonly PackagePublisher Unknown = new("", false);

    public bool IsEmpty => string.IsNullOrWhiteSpace(Name);

    public string DisplayName => IsEmpty ? "Unknown" : Name;

    public string TrustLabel => IsVerified ? "Verified" : "Not verified";
}
