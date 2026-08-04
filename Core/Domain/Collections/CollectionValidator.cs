namespace WinGetXAvalonia.Core.Domain.Collections;

public static class CollectionValidator
{
    public const int MaxNameLength = 64;

    private static readonly char[] InvalidNameChars = ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];

    public static bool IsValidName(string? name)
        => !string.IsNullOrWhiteSpace(name)
           && name.Length <= MaxNameLength
           && name == name.Trim()
           && name.IndexOfAny(InvalidNameChars) < 0;

    public static string NormalizeName(string? name)
        => name?.Trim() ?? "";
}
