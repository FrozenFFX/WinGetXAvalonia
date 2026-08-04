namespace WinGetXAvalonia.Core.Domain.Package;

public sealed record PackageSource(string Name, SourceTrustLevel Trust)
{
    public static readonly PackageSource Unknown = new("", SourceTrustLevel.Unknown);

    public bool IsEmpty => string.IsNullOrWhiteSpace(Name);

    public string TrustLabel => Trust switch
    {
        SourceTrustLevel.High => "High",
        SourceTrustLevel.Medium => "Medium",
        SourceTrustLevel.Low => "Low",
        _ => "Unknown"
    };
}
