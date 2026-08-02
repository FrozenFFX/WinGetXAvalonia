using System.Reflection;

namespace WinGetXAvalonia.Infrastructure;

public static class AppInfo
{
    public const string ProductName = "WinGetX";

    public const string Description =
        "Fast • Clean • Open Source";

    public const string Repository =
        "https://github.com/FrozenFFX/WinGetXAvalonia";

    public const string License = "MIT";

    public static string Version =>
        Assembly.GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString(3) ?? "0.1.0";
}
