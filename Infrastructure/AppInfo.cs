// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Reflection;

namespace WinGetXAvalonia.Infrastructure;

public static class AppInfo
{
    public const string ProductName = "WinGetX";

    public const string Description =
        "Fast • Clean • Reliable";

    public const string Repository =
        "https://github.com/FrozenFFX";

    public const string License = "MIT";

    public static string Version =>
        Assembly.GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString(3) ?? "0.1.0";
}
