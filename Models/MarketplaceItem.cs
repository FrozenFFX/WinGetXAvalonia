// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace WinGetXAvalonia.Models;

public partial class MarketplaceItem : ObservableObject
{
    [ObservableProperty]
    private string id = "";

    [ObservableProperty]
    private MarketplaceKind kind;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private string author = "";

    [ObservableProperty]
    private string category = "";

    [ObservableProperty]
    private string icon = "📦";

    [ObservableProperty]
    private string version = "1.0";

    [ObservableProperty]
    private double rating;

    [ObservableProperty]
    private int ratingCount;

    [ObservableProperty]
    private bool verified;

    [ObservableProperty]
    private long downloads;

    [ObservableProperty]
    private List<string> tags = new();

    [ObservableProperty]
    private string payload = "";

    [ObservableProperty]
    [JsonIgnore]
    [NotifyPropertyChangedFor(nameof(InstallText), nameof(CanInstall))]
    private bool isInstalled;

    [ObservableProperty]
    [JsonIgnore]
    [NotifyPropertyChangedFor(nameof(InstallText), nameof(CanInstall))]
    private bool isInstalling;

    public string RatingDisplay => Rating > 0 ? Rating.ToString("0.0") : "—";

    public string RatingCountDisplay => RatingCount switch
    {
        0 => "",
        1 => "(1)",
        var count => $"({count})"
    };

    public string DownloadsDisplay => Downloads switch
    {
        0 => "",
        < 1000 => Downloads.ToString(),
        < 1_000_000 => $"{Downloads / 1000.0:0.#}K",
        _ => $"{Downloads / 1_000_000.0:0.#}M"
    };

    public bool ShowRating => RatingCount > 0;

    public bool ShowDownloads => Downloads > 0;

    public string InstallText => IsInstalling
        ? "Installing..."
        : IsInstalled
            ? "Installed"
            : "Install";

    public bool CanInstall => !IsInstalling && !IsInstalled;
}
