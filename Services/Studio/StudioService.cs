// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

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
            Icon = "diagnostics",
            Title = "Diagnostics",
            Description = "System health and environment report",
            Category = "System",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "logs",
            Icon = "history",
            Title = "Operation History",
            Description = "Review package operations and their results",
            Category = "System",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "reports",
            Icon = "diagnostics",
            Title = "Reports",
            Description = "Package and usage reports",
            Category = "System",
            Status = StudioToolStatus.ComingSoon
        },
        new()
        {
            Id = "queue",
            Icon = "download",
            Title = "Queue Scheduler",
            Description = "Schedule and prioritize operations",
            Category = "System",
            Status = StudioToolStatus.ComingSoon
        },
        new()
        {
            Id = "security",
            Icon = "security",
            Title = "Security",
            Description = "Package and system security checks",
            Category = "System",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "collections",
            Icon = "workspace",
            Title = "Collections",
            Description = "Group apps into reusable sets",
            Category = "Workflow",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "automation",
            Icon = "settings",
            Title = "Automation",
            Description = "Rules and event triggers",
            Category = "Workflow",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "themes",
            Icon = "studio",
            Title = "Themes",
            Description = "Customize colors and appearance",
            Category = "Customization",
            Status = StudioToolStatus.Ready
        },
        new()
        {
            Id = "plugins",
            Icon = "package",
            Title = "Plugins",
            Description = "Extend WinGetX with add-ons",
            Category = "Customization",
            Status = StudioToolStatus.ComingSoon
        },
        new()
        {
            Id = "founder",
            Icon = "favorite",
            Title = "Founder Center",
            Description = "Exclusive Founder Edition tools",
            Category = "Founder",
            Status = StudioToolStatus.ComingSoon
        }
    };
}
