// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;

namespace WinGetXAvalonia.Core.Domain.Palette;

public static class PaletteQueryParser
{
    public static bool IsCommandMode(string? input)
        => !string.IsNullOrWhiteSpace(input)
           && input!.TrimStart().StartsWith(">", StringComparison.Ordinal);

    public static string CommandText(string? input)
    {
        if (!IsCommandMode(input))
            return input?.Trim() ?? "";

        var trimmed = input!.TrimStart();

        return trimmed.Length <= 1 ? "" : trimmed[1..].Trim();
    }

    public static (string Verb, string Argument) Split(string? text)
    {
        text = text?.Trim() ?? "";

        if (text.Length == 0)
            return ("", "");

        int index = text.IndexOf(' ');

        if (index < 0)
            return (text.ToLowerInvariant(), "");

        return (
            text[..index].Trim().ToLowerInvariant(),
            text[(index + 1)..].Trim());
    }
}
