namespace WinGetXAvalonia.Models;

public sealed class StudioTool
{
    public string Id { get; set; } = "";

    public string Icon { get; set; } = "";

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Category { get; set; } = "";

    public StudioToolStatus Status { get; set; } = StudioToolStatus.Ready;
}
