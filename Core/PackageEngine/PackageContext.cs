namespace WinGetXAvalonia.Core.PackageEngine;

/// <summary>
/// Carries a request and its result through the package pipeline.
/// </summary>
public class PackageContext
{
    public PackageRequest Request { get; }

    public PackageResult Result { get; set; }

    public PackageContext(PackageRequest request)
    {
        Request = request;
        Result = new PackageResult();
    }
}
