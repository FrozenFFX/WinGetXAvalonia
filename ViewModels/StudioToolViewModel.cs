using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public sealed class StudioToolViewModel
{
    public StudioTool Tool { get; }

    public StudioToolViewModel(StudioTool tool)
    {
        Tool = tool;
    }

    public string Icon => Tool.Icon;

    public string Title => Tool.Title;

    public string Description => Tool.Description;

    public string Category => Tool.Category;

    public StudioToolStatus Status => Tool.Status;

    public string Badge => Tool.Status switch
    {
        StudioToolStatus.Experimental => "Experimental",
        StudioToolStatus.ComingSoon => "Coming Soon",
        _ => "Ready"
    };

    public string BadgeStatus => Tool.Status switch
    {
        StudioToolStatus.Experimental => "warning",
        StudioToolStatus.ComingSoon => "info",
        _ => "success"
    };
}
