using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Studio;

public class StudioService : IStudioService
{
    public Task<IReadOnlyList<StudioTool>> GetToolsAsync()
        => Task.FromResult<IReadOnlyList<StudioTool>>(Catalog);

    private static readonly List<StudioTool> Catalog = new()
    {
        new()
        {
            Id = "diagnostics",
            Icon = "🩺",
            Title = "Diagnostics",
            Description = "System health and environment report",
            Category = "System",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "logs",
            Icon = "📜",
            Title = "Log Viewer",
            Description = "App and operation logs",
            Category = "System",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "reports",
            Icon = "📊",
            Title = "Reports",
            Description = "Package and usage reports",
            Category = "System",
            Status = StudioToolStatus.Experimental
        },
        new()
        {
            Id = "queue",
            Icon = "📥",
            Title = "Queue Scheduler",
            Description = "Schedule and prioritize operations",
            Category = "System",
            Status = StudioToolStatus.Experimental
        },
        new()
        {
            Id = "security",
            Icon = "🛡",
            Title = "Security",
            Description = "Package and system security checks",
            Category = "System",
            Status = StudioToolStatus.Experimental
        },
        new()
        {
            Id = "collections",
            Icon = "🗂",
            Title = "Collections",
            Description = "Group apps into reusable sets",
            Category = "Workflow",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "automation",
            Icon = "⚙",
            Title = "Automation",
            Description = "Rules and event triggers",
            Category = "Workflow",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "themes",
            Icon = "🎨",
            Title = "Themes",
            Description = "Customize colors and appearance",
            Category = "Customization",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "plugins",
            Icon = "🧩",
            Title = "Plugins",
            Description = "Extend WinGetX with add-ons",
            Category = "Customization",
            Status = StudioToolStatus.ComingSoon
        },
        new()
        {
            Id = "founder",
            Icon = "👑",
            Title = "Founder Center",
            Description = "Exclusive Founder Edition tools",
            Category = "Founder",
            Status = StudioToolStatus.ComingSoon
        }
    };
}
