// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.Domain.Package;

public sealed record PackageManifest(
    string Name,
    string Description,
    string Homepage,
    string License,
    string ReleaseNotes,
    string Icon)
{
    public static readonly PackageManifest None = new("", "", "", "", "", "");

    public bool IsEmpty => string.IsNullOrWhiteSpace(Name);
}
