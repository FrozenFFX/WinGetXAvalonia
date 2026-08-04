namespace WinGetXAvalonia.Core.Runtime;

public sealed class RuntimeFeature
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool Enabled { get; set; }

    public bool Experimental { get; set; }
}
