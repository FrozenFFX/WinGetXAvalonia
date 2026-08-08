// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.Core.IntentEngine;

public interface IUserIntentEngine
{
    Task<IntentResult> AnalyzeUserIntentAsync(string userInput, UserContext context);
    Task<IEnumerable<PackageRecommendation>> GetRecommendationsAsync(IntentResult intent);
    Task<AutomationPlan> CreateAutomationPlanAsync(IntentResult intent);
    Task<IEnumerable<string>> GenerateCommandsAsync(IntentResult intent);
    Task<IEnumerable<string>> GetRelatedPackagesAsync(IntentResult intent);
    Task<IEnumerable<string>> GetRelatedDocumentationAsync(IntentResult intent);
}

public class IntentResult
{
    public IntentType IntentType { get; set; }
    public string Description { get; set; } = string.Empty;
    public IntentCategory Category { get; set; }
    public IntentPriority Priority { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<string> InstalledPackages { get; set; } = new();
    public IntentScope Scope { get; set; }
}

public enum IntentType
{
    Install,
    Update,
    Remove,
    Search,
    Configure,
    Debug,
    Learn,
    Collaborate,
    Automate,
    Backup,
    Restore
}

public enum IntentCategory
{
    Development,
    Gaming,
    Productivity,
    Multimedia,
    System,
    Utilities,
    Security,
    DevelopmentTools,
    Database,
    Network,
    Design,
    Testing,
    Deployment,
    Monitoring
}

public enum IntentPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum IntentScope
{
    Local,
    Workspace,
    System,
    Project,
    Organization
}

public class UserContext
{
    public string UserId { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public List<string> InstalledPackages { get; set; } = new();
    public List<string> RecentActivities { get; set; } = new();
    public string CurrentProject { get; set; } = string.Empty;
    public string DevelopmentStack { get; set; } = string.Empty;
    public IntentScope DefaultScope { get; set; }
    public bool IsDeveloper { get; set; }
    public bool IsGaming { get; set; }
}

public class PackageRecommendation
{
    public string PackageId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationType RecommendationType { get; set; }
    public double ConfidenceScore { get; set; }
    public string[] RelatedPackages { get; set; } = Array.Empty<string>();
    public string[] RequiredPackages { get; set; } = Array.Empty<string>();
    public string[] OptionalPackages { get; set; } = Array.Empty<string>();
    public string[] InstallationSteps { get; set; } = Array.Empty<string>();
    public string[] ConfigurationSteps { get; set; } = Array.Empty<string>();
    public string[] DocumentationLinks { get; set; } = Array.Empty<string>();
}

public enum RecommendationType
{
    Essential,
    Optional,
    Suggested,
    Alternative
}

public class AutomationPlan
{
    public string PlanId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<AutomationStep> Steps { get; set; } = new();
    public AutomationPlanType PlanType { get; set; }
    public string EstimatedTime { get; set; } = string.Empty;
    public string RequiredPermissions { get; set; } = string.Empty;
}

public class AutomationStep
{
    public int StepNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public string[] Commands { get; set; } = Array.Empty<string>();
    public AutomationStepType StepType { get; set; }
    public bool IsRequired { get; set; }
    public int EstimatedTime { get; set; }
}

public enum AutomationStepType
{
    PackageInstall,
    PackageConfigure,
    SystemSetup,
    FileOperation,
    ServiceOperation,
    EnvironmentSetup
}

public enum AutomationPlanType
    {
        DevelopmentEnvironment,
        GamingSetup,
        MultimediaSetup,
        SystemMaintenance,
        DevelopmentWorkflow,
        CI_CDSetup,
        Deployment,
        BackupRecovery
    }

public class IntentEngine : IUserIntentEngine
{
    private readonly IPackageProvider _packageProvider;
    private readonly IHistoryService _historyService;
    private readonly NotificationService _notifications;
    private readonly LoggerService _logger;
    private readonly Dictionary<string, List<IntentPattern>> _intentPatterns;
    private readonly Dictionary<string, List<PackageRecommendation>> _packageRecommendations;
    private readonly Dictionary<string, List<AutomationPlan>> _automationPlans;

    public IntentEngine(
        IPackageProvider packageProvider,
        IHistoryService historyService,
        NotificationService notifications,
        LoggerService logger)
    {
        _packageProvider = packageProvider;
        _historyService = historyService;
        _notifications = notifications;
        _logger = logger;

        _intentPatterns = InitializeIntentPatterns()
            .GroupBy(p => p.Name)
            .ToDictionary(g => g.Key, g => g.ToList());
        _packageRecommendations = InitializePackageRecommendations();
        _automationPlans = InitializeAutomationPlans();
    }

    public async Task<IntentResult> AnalyzeUserIntentAsync(string userInput, UserContext context)
    {
        var intentResult = new IntentResult
        {
            Confidence = 0.0,
            PackageId = ExtractPackageId(userInput),
            Version = ExtractVersion(userInput),
            Description = userInput
        };

        var normalizedInput = NormalizeInput(userInput);

        foreach (var pattern in _intentPatterns.Values.SelectMany(p => p))
        {
            var matches = pattern.Match(normalizedInput, context);
            if (matches > 0 && matches >= pattern.MinConfidence)
            {
                intentResult.IntentType = pattern.IntentType;
                intentResult.Category = pattern.Category;
                intentResult.Priority = pattern.Priority;
                intentResult.Confidence = (double)matches / pattern.MaxConfidence;

                intentResult.Parameters = pattern.ExtractParameters(normalizedInput);
                intentResult.Scope = DetermineScope(context, pattern);

                _logger.Log($"Intent analysis: Matched pattern '{pattern.Name}' with confidence {intentResult.Confidence:F2}");
                return intentResult;
            }
        }

        intentResult.IntentType = IntentType.Search;
        intentResult.Category = IntentCategory.Development;
        intentResult.Priority = IntentPriority.Medium;
        intentResult.Confidence = 0.5;

        return intentResult;
    }

    public async Task<IEnumerable<PackageRecommendation>> GetRecommendationsAsync(IntentResult intent)
    {
        var recommendations = new List<PackageRecommendation>();

        if (_packageRecommendations.ContainsKey(intent.IntentType.ToString()))
        {
            recommendations.AddRange(_packageRecommendations[intent.IntentType.ToString()]);
        }

        if (_packageRecommendations.ContainsKey(intent.Category.ToString()))
        {
            var categoryRecommendations = _packageRecommendations[intent.Category.ToString()]
                .Where(r => r.RecommendationType != RecommendationType.Alternative)
                .ToList();
            recommendations.AddRange(categoryRecommendations);
        }

        var contextBasedRecommendations = await GetContextBasedRecommendationsAsync(intent);
        recommendations.AddRange(contextBasedRecommendations);

        recommendations = recommendations.OrderByDescending(r => r.ConfidenceScore).Take(10).ToList();

        return recommendations;
    }

    public async Task<AutomationPlan> CreateAutomationPlanAsync(IntentResult intent)
    {
        if (_automationPlans.ContainsKey(intent.IntentType.ToString()))
        {
            var plans = _automationPlans[intent.IntentType.ToString()];
            return plans.FirstOrDefault() ?? CreateDefaultPlan(intent);
        }

        if (_automationPlans.ContainsKey(intent.Category.ToString()))
        {
            var plans = _automationPlans[intent.Category.ToString()];
            return plans.FirstOrDefault() ?? CreateDefaultPlan(intent);
        }

        return CreateDefaultPlan(intent);
    }

    public async Task<IEnumerable<string>> GenerateCommandsAsync(IntentResult intent)
    {
        var commands = new List<string>();

        switch (intent.IntentType)
        {
            case IntentType.Install:
                commands.AddRange(await GenerateInstallCommandsAsync(intent));
                break;
            case IntentType.Update:
                commands.AddRange(await GenerateUpdateCommandsAsync(intent));
                break;
            case IntentType.Remove:
                commands.AddRange(await GenerateRemoveCommandsAsync(intent));
                break;
            case IntentType.Configure:
                commands.AddRange(await GenerateConfigureCommandsAsync(intent));
                break;
            case IntentType.Debug:
                commands.AddRange(await GenerateDebugCommandsAsync(intent));
                break;
            case IntentType.Learn:
                commands.AddRange(await GenerateLearnCommandsAsync(intent));
                break;
            case IntentType.Collaborate:
                commands.AddRange(await GenerateCollaborateCommandsAsync(intent));
                break;
            case IntentType.Automate:
                commands.AddRange(await GenerateAutomateCommandsAsync(intent));
                break;
            case IntentType.Backup:
                commands.AddRange(await GenerateBackupCommandsAsync(intent));
                break;
            case IntentType.Restore:
                commands.AddRange(await GenerateRestoreCommandsAsync(intent));
                break;
            default:
                commands.AddRange(await GenerateSearchCommandsAsync(intent));
                break;
        }

        return commands;
    }

    public async Task<IEnumerable<string>> GetRelatedPackagesAsync(IntentResult intent)
    {
        var relatedPackages = new List<string>();

        if (!string.IsNullOrWhiteSpace(intent.PackageId))
        {
            relatedPackages.AddRange(await GetPackageDependenciesAsync(intent.PackageId));
            relatedPackages.AddRange(await GetComplementaryPackagesAsync(intent.PackageId));
            relatedPackages.AddRange(await GetSimilarPackagesAsync(intent.PackageId));
        }

        return relatedPackages.Distinct();
    }

    public async Task<IEnumerable<string>> GetRelatedDocumentationAsync(IntentResult intent)
    {
        var documentation = new List<string>
        {
            $"https://learn.microsoft.com/en-us/windows/package-manager/winget/",
            $"https://docs.microsoft.com/en-us/windows/package-manager/",
            $"https://github.com/microsoft/-winget-cli"
        };

        switch (intent.Category)
        {
            case IntentCategory.Development:
                documentation.AddRange(new[]
                {
                    "https://learn.microsoft.com/en-us/visualstudio/",
                    "https://docs.microsoft.com/en-us/dotnet/",
                    "https://docs.microsoft.com/en-us/windows/terminal/"
                });
                break;
            case IntentCategory.Gaming:
                documentation.AddRange(new[]
                {
                    "https://docs.unity3d.com/",
                    "https://docs.unrealengine.com/",
                    "https://docs.figma.com/"
                });
                break;
            case IntentCategory.Multimedia:
                documentation.AddRange(new[]
                {
                    "https://docs.vlc.media/",
                    "https://docs.spotify.com/",
                    "https://docs.obsstudio.org/"
                });
                break;
            case IntentCategory.Design:
                documentation.AddRange(new[]
                {
                    "https://docs.figma.com/",
                    "https://docs.adobe.com/",
                    "https://docs.blender.org/"
                });
                break;
        }

        return documentation;
    }

    private IntentScope DetermineScope(UserContext context, IntentPattern pattern)
    {
        if (pattern.AllowedScopes.Contains(IntentScope.Local) && context.DefaultScope == IntentScope.Local)
            return IntentScope.Local;

        if (pattern.AllowedScopes.Contains(IntentScope.Workspace) && !string.IsNullOrWhiteSpace(context.CurrentProject))
            return IntentScope.Workspace;

        if (pattern.AllowedScopes.Contains(IntentScope.System))
            return IntentScope.System;

        return context.DefaultScope;
    }

    private async Task<List<PackageRecommendation>> GetContextBasedRecommendationsAsync(IntentResult intent)
    {
        var recommendations = new List<PackageRecommendation>();

        foreach (var installedPackage in intent.InstalledPackages)
        {
            var complementary = await GetComplementaryPackagesForCategoryAsync(intent.Category, installedPackage);
            recommendations.AddRange(complementary);
        }

        return recommendations;
    }

    private async Task<IEnumerable<string>> GetPackageDependenciesAsync(string packageId)
    {
        var dependencies = new List<string>();

        try
        {
            var allPackages = await _packageProvider.GetInstalledAsync();

            var package = allPackages.FirstOrDefault(p =>
                p.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase));

            if (package is null)
                _logger.Log($"No installed package record for '{packageId}'");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to get dependencies for package: {packageId}");
        }

        return dependencies;
    }

    private async Task<IEnumerable<string>> GetComplementaryPackagesAsync(string packageId)
    {
        var complementaryPackages = new List<string>
        {
            "git",
            "docker",
            "nodejs",
            "python",
            "visual-studio-code",
            "notepad++",
            "git-lfs"
        };

        return complementaryPackages;
    }

    private async Task<IEnumerable<string>> GetSimilarPackagesAsync(string packageId)
    {
        var similarPackages = new List<string>
        {
            $"{packageId}-extension",
            $"{packageId}-helper",
            $"{packageId}-tools",
            $"{packageId}-utils"
        };

        return similarPackages;
    }

    private async Task<IEnumerable<PackageRecommendation>> GetComplementaryPackagesForCategoryAsync(IntentCategory category, string installedPackage)
    {
        var recommendations = new List<PackageRecommendation>
        {
            new PackageRecommendation
            {
                PackageId = "vscode",
                Name = "Visual Studio Code",
                Description = "Professional code editor",
                RecommendationType = RecommendationType.Suggested,
                ConfidenceScore = 0.8
            },
            new PackageRecommendation
            {
                PackageId = "git",
                Name = "Git",
                Description = "Version control system",
                RecommendationType = RecommendationType.Essential,
                ConfidenceScore = 0.9
            },
            new PackageRecommendation
            {
                PackageId = "docker",
                Name = "Docker",
                Description = "Containerization platform",
                RecommendationType = RecommendationType.Optional,
                ConfidenceScore = 0.7
            }
        };

        return recommendations;
    }

    private List<IntentPattern> InitializeIntentPatterns()
    {
        var patterns = new List<IntentPattern>();

        patterns.Add(new IntentPattern
        {
            Name = "Install Package",
            IntentType = IntentType.Install,
            Category = IntentCategory.Development,
            Priority = IntentPriority.High,
            MinConfidence = 80,
            MaxConfidence = 100,
            Keywords = new[] { "install", "setup", "add", "deploy", "configure" },
            Pattern = @"(?i)(install|setup|add|deploy|configure)\s+(.+?)(?:\s+(?:version|v=)?(.+?))?$",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Local }
        });

        patterns.Add(new IntentPattern
        {
            Name = "Update Package",
            IntentType = IntentType.Update,
            Category = IntentCategory.Development,
            Priority = IntentPriority.Medium,
            MinConfidence = 70,
            MaxConfidence = 100,
            Keywords = new[] { "update", "upgrade", "refresh", "latest" },
            Pattern = @"(?i)(update|upgrade|refresh|latest)\s+(.+?)(?:\s+(?:version|v=)?(.+?))?$,",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Local }
        });

        patterns.Add(new IntentPattern
        {
            Name = "Remove Package",
            IntentType = IntentType.Remove,
            Category = IntentCategory.Development,
            Priority = IntentPriority.Low,
            MinConfidence = 60,
            MaxConfidence = 100,
            Keywords = new[] { "remove", "uninstall", "delete", "purge" },
            Pattern = @"(?i)(remove|uninstall|delete|purge)\s+(.+?)$",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Local }
        });

        patterns.Add(new IntentPattern
        {
            Name = "Search for Package",
            IntentType = IntentType.Search,
            Category = IntentCategory.Development,
            Priority = IntentPriority.Low,
            MinConfidence = 50,
            MaxConfidence = 100,
            Keywords = new[] { "search", "find", "locate", "discover", "help" },
            Pattern = @"(?i)(search|find|locate|discover|help)\s+(.+?)$",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Local }
        });

        patterns.Add(new IntentPattern
        {
            Name = "Configure Development",
            IntentType = IntentType.Configure,
            Category = IntentCategory.Development,
            Priority = IntentPriority.Medium,
            MinConfidence = 70,
            MaxConfidence = 100,
            Keywords = new[] { "setup", "configure", "initialize", "project" },
            Pattern = @"(?i)(setup|configure|initialize|project)\s+(.+?)$",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Project }
        });

        patterns.Add(new IntentPattern
        {
            Name = "Debug Environment",
            IntentType = IntentType.Debug,
            Category = IntentCategory.Development,
            Priority = IntentPriority.Medium,
            MinConfidence = 60,
            MaxConfidence = 100,
            Keywords = new[] { "debug", "diagnose", "troubleshoot", "fix" },
            Pattern = @"(?i)(debug|diagnose|troubleshoot|fix)\s*(.+?)?$",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Local }
        });

        patterns.Add(new IntentPattern
        {
            Name = "Learn Development",
            IntentType = IntentType.Learn,
            Category = IntentCategory.Development,
            Priority = IntentPriority.Low,
            MinConfidence = 40,
            MaxConfidence = 100,
            Keywords = new[] { "learn", "tutorial", "documentation", "guide", "help" },
            Pattern = @"(?i)(learn|tutorial|documentation|guide|help)\s*(.+?)?$",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Local }
        });

        patterns.Add(new IntentPattern
        {
            Name = "Gaming Setup",
            IntentType = IntentType.Install,
            Category = IntentCategory.Gaming,
            Priority = IntentPriority.High,
            MinConfidence = 80,
            MaxConfidence = 100,
            Keywords = new[] { "gaming", "game", "steam", "epic", "playstation", "xbox" },
            Pattern = @"(?i)(gaming|game|steam|epic|playstation|xbox)\s*(.+?)?$",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Local }
        });

        patterns.Add(new IntentPattern
        {
            Name = "Multimedia Setup",
            IntentType = IntentType.Install,
            Category = IntentCategory.Multimedia,
            Priority = IntentPriority.Medium,
            MinConfidence = 70,
            MaxConfidence = 100,
            Keywords = new[] { "media", "video", "audio", "music", "video player", "vlc", "spotify" },
            Pattern = @"(?i)(media|video|audio|music|video player|vlc|spotify)\s*(.+?)?$",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Local }
        });

        patterns.Add(new IntentPattern
        {
            Name = "Design Tools",
            IntentType = IntentType.Install,
            Category = IntentCategory.Design,
            Priority = IntentPriority.Medium,
            MinConfidence = 70,
            MaxConfidence = 100,
            Keywords = new[] { "design", "design tool", "figma", "sketch", "adobe", "photoshop" },
            Pattern = @"(?i)(design|design tool|figma|sketch|adobe|photoshop)\s*(.+?)?$",
            AllowedScopes = new[] { IntentScope.Workspace, IntentScope.Local }
        });

        return patterns;
    }

    private Dictionary<string, List<PackageRecommendation>> InitializePackageRecommendations()
    {
        var recommendations = new Dictionary<string, List<PackageRecommendation>>();

        recommendations[IntentType.Install.ToString()] = new List<PackageRecommendation>
        {
            new PackageRecommendation
            {
                PackageId = "vscode",
                Name = "Visual Studio Code",
                Description = "Professional code editor with IntelliSense",
                RecommendationType = RecommendationType.Essential,
                ConfidenceScore = 0.95,
                RequiredPackages = new[] { "dotnet-sdk", "git" },
                OptionalPackages = new[] { "docker", "nodejs" },
                InstallationSteps = new[]
                {
                    "Download and install Visual Studio Code",
                    "Install .NET SDK",
                    "Configure Git"
                },
                ConfigurationSteps = new[]
                {
                    "Set up workspace",
                    "Install extensions: C#, Python, GitHub",
                    "Configure terminal settings"
                },
                DocumentationLinks = new[]
                {
                    "https://code.visualstudio.com/docs",
                    "https://docs.microsoft.com/en-us/visualstudio/"
                }
            },
            new PackageRecommendation
            {
                PackageId = "git",
                Name = "Git",
                Description = "Distributed version control system",
                RecommendationType = RecommendationType.Essential,
                ConfidenceScore = 0.98,
                RequiredPackages = new string[0],
                OptionalPackages = new[] { "git-lfs", "github-cli" },
                InstallationSteps = new[] { "Install Git" },
                DocumentationLinks = new[] { "https://git-scm.com/docs" }
            },
            new PackageRecommendation
            {
                PackageId = "dotnet-sdk",
                Name = ".NET SDK",
                Description = "Develop modern cross-platform applications",
                RecommendationType = RecommendationType.Essential,
                ConfidenceScore = 0.9,
                RequiredPackages = new[] { "git" },
                InstallationSteps = new[] { "Install .NET SDK" },
                DocumentationLinks = new[] { "https://docs.microsoft.com/en-us/dotnet/" }
            }
        };

        recommendations[IntentType.Update.ToString()] = new List<PackageRecommendation>
        {
            new PackageRecommendation
            {
                PackageId = "windows-terminal",
                Name = "Windows Terminal",
                Description = "Modern terminal with tabs and customization",
                RecommendationType = RecommendationType.Suggested,
                ConfidenceScore = 0.8,
                InstallationSteps = new[] { "Update Windows Terminal" },
                DocumentationLinks = new[] { "https://aka.ms/terminal-docs" }
            },
            new PackageRecommendation
            {
                PackageId = "dotnet-runtime",
                Name = ".NET Runtime",
                Description = "Run .NET applications",
                RecommendationType = RecommendationType.Optional,
                ConfidenceScore = 0.7,
                InstallationSteps = new[] { "Update .NET Runtime" },
                DocumentationLinks = new[] { "https://docs.microsoft.com/en-us/dotnet/" }
            }
        };

        recommendations[IntentType.Remove.ToString()] = new List<PackageRecommendation>
        {
            new PackageRecommendation
            {
                PackageId = "old-unused-package",
                Name = "Old Unused Package",
                Description = "This package is no longer needed",
                RecommendationType = RecommendationType.Alternative,
                ConfidenceScore = 0.6,
                InstallationSteps = new[] { "Remove the package" },
                DocumentationLinks = new string[0]
            }
        };

        recommendations[IntentCategory.Development.ToString()] = new List<PackageRecommendation>
        {
            new PackageRecommendation
            {
                PackageId = "docker",
                Name = "Docker",
                Description = "Containerization platform for development",
                RecommendationType = RecommendationType.Optional,
                ConfidenceScore = 0.8,
                RequiredPackages = new[] { "git", "visual-studio-code" },
                InstallationSteps = new[]
                {
                    "Install Docker Desktop",
                    "Configure Docker settings",
                    "Set up container workspace"
                },
                DocumentationLinks = new[]
                {
                    "https://docs.docker.com/get-started/",
                    "https://docs.docker.com/compose/"
                }
            },
            new PackageRecommendation
            {
                PackageId = "nodejs",
                Name = "Node.js",
                Description = "JavaScript runtime for web development",
                RecommendationType = RecommendationType.Suggested,
                ConfidenceScore = 0.85,
                InstallationSteps = new[] { "Install Node.js" },
                DocumentationLinks = new[] { "https://nodejs.org/docs/" }
            },
            new PackageRecommendation
            {
                PackageId = "python",
                Name = "Python",
                Description = "Programming language for development",
                RecommendationType = RecommendationType.Suggested,
                ConfidenceScore = 0.9,
                InstallationSteps = new[] { "Install Python" },
                DocumentationLinks = new[] { "https://docs.python.org/" }
            }
        };

        return recommendations;
    }

    private Dictionary<string, List<AutomationPlan>> InitializeAutomationPlans()
    {
        var plans = new Dictionary<string, List<AutomationPlan>>();

        plans[IntentType.Configure.ToString()] = new List<AutomationPlan>
        {
            new AutomationPlan
            {
                PlanId = "dev-environment-setup",
                Name = "Development Environment Setup",
                Description = "Automatically configure a complete development environment",
                PlanType = AutomationPlanType.DevelopmentEnvironment,
                EstimatedTime = "15-30 minutes",
                RequiredPermissions = "Administrator",
                Steps = new List<AutomationStep>
                {
                    new AutomationStep
                    {
                        StepNumber = 1,
                        Description = "Install essential development tools",
                        StepType = AutomationStepType.PackageInstall,
                        IsRequired = true,
                        EstimatedTime = 5,
                        Commands = new[]
                        {
                            "winget install -e --id Microsoft.VisualStudioCode",
                            "winget install -e --id Git.Git",
                            "winget install -e --id Microsoft.DotNet.SDK"
                        }
                    },
                    new AutomationStep
                    {
                        StepNumber = 2,
                        Description = "Configure development environment",
                        StepType = AutomationStepType.EnvironmentSetup,
                        IsRequired = true,
                        EstimatedTime = 10,
                        Commands = new[]
                        {
                            "git config --global user.name 'Your Name'",
                            "git config --global user.email 'your.email@example.com'",
                            "code --install-extension ms-vscode.cpptools",
                            "code --install-extension ms-python.python"
                        }
                    },
                    new AutomationStep
                    {
                        StepNumber = 3,
                        Description = "Set up workspace files",
                        StepType = AutomationStepType.FileOperation,
                        IsRequired = false,
                        EstimatedTime = 5,
                        Commands = new[]
                        {
                            "mkdir ~/projects",
                            "cd ~/projects",
                            "git clone https://github.com/microsoft/vscode.git",
                            "git clone https://github.com/dotnet/runtime.git"
                        }
                    }
                }
            }
        };

        plans[IntentType.Debug.ToString()] = new List<AutomationPlan>
        {
            new AutomationPlan
            {
                PlanId = "environment-diagnosis",
                Name = "Environment Diagnosis",
                Description = "Diagnose and fix common development environment issues",
                PlanType = AutomationPlanType.SystemMaintenance,
                EstimatedTime = "5-10 minutes",
                RequiredPermissions = "Administrator",
                Steps = new List<AutomationStep>
                {
                    new AutomationStep
                    {
                        StepNumber = 1,
                        Description = "Check system requirements",
                        StepType = AutomationStepType.SystemSetup,
                        IsRequired = true,
                        EstimatedTime = 3,
Commands = new[]
                        {
                            "systeminfo | findstr /B /C:\"OS Name\" /C:\"OS Version\"",
                            "wmic cpu get name",
                            "wmic computersystem get totalphysicalmemory"
                        }
                    },
                    new AutomationStep
                    {
                        StepNumber = 2,
                        Description = "Check installed packages",
                        StepType = AutomationStepType.PackageInstall,
                        IsRequired = true,
                        EstimatedTime = 3,
                        Commands = new[]
                        {
                            "winget list --all",
                            "where dotnet",
                            "where python",
                            "where node"
                        }
                    }
                }
            }
        };

        return plans;
    }

    private AutomationPlan CreateDefaultPlan(IntentResult intent)
    {
        return new AutomationPlan
        {
            PlanId = "generic-setup",
            Name = $"{intent.IntentType} Plan",
            Description = $"Generic plan for {intent.IntentType} operation",
            PlanType = AutomationPlanType.Deployment,
            EstimatedTime = "Unknown",
            RequiredPermissions = "Administrator",
            Steps = new List<AutomationStep>
            {
                new AutomationStep
                {
                    StepNumber = 1,
                    Description = $"Perform {intent.IntentType} operation",
                    StepType = AutomationStepType.PackageInstall,
                    IsRequired = true,
                    EstimatedTime = 10,
                    Commands = new[] { $"winget {intent.IntentType.ToString().ToLower()} {intent.PackageId}" }
                }
            }
        };
    }

    private async Task<IEnumerable<string>> GenerateInstallCommandsAsync(IntentResult intent)
    {
        var commands = new List<string>
        {
            $"winget install --id {intent.PackageId} --accept-package-agreements"
        };

        if (!string.IsNullOrWhiteSpace(intent.Version))
        {
            commands.Add($"--version {intent.Version}");
        }

        commands.AddRange(new[]
        {
            $"winget install --id {intent.PackageId} --accept-source-agreements",
            $"# Additional recommended packages based on intent category",
            $"winget install --id vscode --accept-package-agreements",
            $"winget install --id git --accept-package-agreements"
        });

        return commands;
    }

    private async Task<IEnumerable<string>> GenerateUpdateCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"winget upgrade --id {intent.PackageId}",
            $"winget upgrade --all",
            $"# Update system packages",
            $"winget install --id dotnet-runtime --accept-package-agreements"
        };
    }

    private async Task<IEnumerable<string>> GenerateRemoveCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"winget uninstall --id {intent.PackageId}",
            $"# Remove associated files and cache",
            "rm -rf ~/.cache/winget",
            @"del %LocalAppData%\Packages\MicrosoftCorporationII.*"
        };
    }

    private async Task<IEnumerable<string>> GenerateConfigureCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"# Configure {intent.PackageId} for development",
            $"mkdir -p ~/projects/{intent.PackageId.ToLower()}",
            $"cd ~/projects/{intent.PackageId.ToLower()}",
            $"git init",
            $"git remote add origin https://github.com/example/{intent.PackageId.ToLower()}.git"
        };
    }

    private async Task<IEnumerable<string>> GenerateDebugCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"# Debug environment",
            "systeminfo",
            $"winget list --all",
            $"where {intent.PackageId}",
            $"# Check for updates",
            $"winget upgrade"
        };
    }

    private async Task<IEnumerable<string>> GenerateLearnCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"# Get documentation for {intent.PackageId}",
            $"winget show --id {intent.PackageId}",
            $"# Find community examples",
            $"gh search {intent.PackageId}",
            $"# Join community forums",
            $"# Schedule regular updates"
        };
    }

    private async Task<IEnumerable<string>> GenerateCollaborateCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"# Set up collaboration tools",
            "winget install --id github-cli",
            $"winget install --id slack",
            $"winget install --id microsoft-teams",
            $"# Configure Git for collaboration",
            "git config --global core.autocrlf true",
            "git config --global pull.rebase true"
        };
    }

    private async Task<IEnumerable<string>> GenerateAutomateCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"# Create automation scripts",
            "mkdir -p ~/.scripts",
            $"# Create setup script for {intent.PackageId}",
            $"cat > ~/scripts/setup-{intent.PackageId.ToLower()}.sh << 'EOF'",
            "#!/bin/bash",
            $"# Installation and configuration steps",
            "EOF",
            "chmod +x ~/scripts/setup-{intent.PackageId.ToLower()}.sh"
        };
    }

    private async Task<IEnumerable<string>> GenerateBackupCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"# Create backup of {intent.PackageId} configuration",
            "winget export --output ~/backups/winget-backup.json",
            $"# Backup important files",
            "rsync -av ~/.config/{intent.PackageId.ToLower()}/ ~/backups/{intent.PackageId.ToLower()}-config/",
            $"# Set up automatic backup schedule",
            "echo \"0 2 * * * winget export --output ~/backups/winget-backup-$(date +%Y%m%d).json\" | crontab -"
        };
    }

    private async Task<IEnumerable<string>> GenerateRestoreCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"# Restore {intent.PackageId} from backup",
            "winget import --file ~/backups/winget-backup.json",
            $"# Restore configuration",
            "rsync -av ~/backups/{intent.PackageId.ToLower()}-config/ ~/.config/{intent.PackageId.ToLower()}/",
            $"# Verify installation",
            $"winget list --id {intent.PackageId}"
        };
    }

    private async Task<IEnumerable<string>> GenerateSearchCommandsAsync(IntentResult intent)
    {
        return new List<string>
        {
            $"winget search '{intent.PackageId}'",
            $"winget show --id {intent.PackageId}",
            $"# Find similar packages",
            $"winget search --query '{intent.Description}' --tag examples"
        };
    }

    private string NormalizeInput(string input)
    {
        return input.Trim().Replace("  ", " ").Replace("$", "$").Replace("#", "#");
    }

    private static string ExtractPackageId(string input)
    {
        var packageIdPattern = @"(?i)\b([a-z0-9-]+)+\b";
        var match = Regex.Match(input, packageIdPattern);
        return match.Success ? match.Value : string.Empty;
    }

    private static string ExtractVersion(string input)
    {
        var versionPattern = @"(?i)(?:version|v|=)\s*([0-9.]+)";
        var match = Regex.Match(input, versionPattern);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private class IntentPattern
    {
        public string Name { get; set; } = string.Empty;
        public IntentType IntentType { get; set; }
        public IntentCategory Category { get; set; }
        public IntentPriority Priority { get; set; }
        public int MinConfidence { get; set; }
        public int MaxConfidence { get; set; }
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public string Pattern { get; set; } = string.Empty;
        public IntentScope[] AllowedScopes { get; set; } = Array.Empty<IntentScope>();

        public int Match(string input, UserContext context)
        {
            var score = 0;

            foreach (var keyword in Keywords)
            {
                if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10;
                }
            }

            if (!string.IsNullOrWhiteSpace(Pattern))
            {
                var regex = new Regex(Pattern, RegexOptions.IgnoreCase);
                if (regex.IsMatch(input))
                {
                    score += 20;
                }
            }

            if (context.IsDeveloper && Category == IntentCategory.Development)
            {
                score += 15;
            }

            if (context.IsGaming && Category == IntentCategory.Gaming)
            {
                score += 15;
            }

            return score;
        }

        public Dictionary<string, object> ExtractParameters(string input)
        {
            var parameters = new Dictionary<string, object>();

            var packageId = IntentEngine.ExtractPackageId(input);
            if (!string.IsNullOrWhiteSpace(packageId))
            {
                parameters["PackageId"] = packageId;
            }

            var version = IntentEngine.ExtractVersion(input);
            if (!string.IsNullOrWhiteSpace(version))
            {
                parameters["Version"] = version;
            }

            return parameters;
        }
    }
}
