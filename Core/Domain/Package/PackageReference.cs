namespace WinGetXAvalonia.Core.Domain.Package;

public sealed record PackageReference(PackageIdentity Identity, string Version)
{
    public static readonly PackageReference None = new(PackageIdentity.None, "");

    public string Id => Identity.Id;

    public string Source => Identity.Source;

    public bool IsEmpty => Identity.IsEmpty;

    public string DisplayName => Identity.DisplayName;

    public static PackageReference Create(string id, string source, string version)
        => new(new PackageIdentity(id, source), version);
}
