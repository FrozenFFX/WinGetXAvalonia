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
}
