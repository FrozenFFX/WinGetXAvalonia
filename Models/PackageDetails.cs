// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public sealed record PackageDetails
{
    public string Name { get; init; } = "";

    public string Id { get; init; } = "";

    public string Version { get; init; } = "";

    public string LatestVersion { get; init; } = "";

    public string Source { get; init; } = "";

    public string Publisher { get; init; } = "";

    public string Description { get; init; } = "";

    public string Homepage { get; init; } = "";

    public string License { get; init; } = "";

    public string ReleaseNotes { get; init; } = "";

    public string Architecture { get; init; } = "";

    public string Scope { get; init; } = "";

    public string InstallerType { get; init; } = "";

    public string InstallerUrl { get; init; } = "";

    public string InstallerSha256 { get; init; } = "";

    public List<string> Dependencies { get; init; } = new();

    public List<string> Tags { get; init; } = new();
}
