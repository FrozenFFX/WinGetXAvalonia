// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Helpers;

public static class IconProvider
{
    public static string Get(string package)
    {
        package = package.ToLower();

        if (package.Contains("chrome"))
            return "🌐";

        if (package.Contains("firefox"))
            return "🦊";

        if (package.Contains("edge"))
            return "🌎";

        if (package.Contains("discord"))
            return "💬";

        if (package.Contains("spotify"))
            return "🎵";

        if (package.Contains("steam"))
            return "🎮";

        if (package.Contains("visual"))
            return "💻";

        if (package.Contains("docker"))
            return "🐳";

        return "📦";
    }
}
