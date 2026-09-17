// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Helpers;

public static class IconProvider
{
    public static string Get(string package)
    {
        package = package.ToLower();

        if (package.Contains("chrome"))
            return "source";

        if (package.Contains("firefox"))
            return "developer";

        if (package.Contains("edge"))
            return "source";

        if (package.Contains("discord"))
            return "notification";

        if (package.Contains("spotify"))
            return "studio";

        if (package.Contains("steam"))
            return "automation";

        if (package.Contains("visual"))
            return "workspace";

        if (package.Contains("docker"))
            return "enterprise";

        return "package";
    }
}
