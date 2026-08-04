// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public class PackageDetailsModel
{
    public string Name { get; set; } = "";

    public string Id { get; set; } = "";

    public string Publisher { get; set; } = "";

    public string Homepage { get; set; } = "";

    public string License { get; set; } = "";

    public string InstalledVersion { get; set; } = "";

    public string LatestVersion { get; set; } = "";

    public string Source { get; set; } = "";

    public string Description { get; set; } = "";

    public List<string> Tags { get; set; } = new();

    public List<string> Dependencies { get; set; } = new();
}
