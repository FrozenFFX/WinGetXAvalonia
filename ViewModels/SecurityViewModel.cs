// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;

namespace WinGetXAvalonia.ViewModels;

public partial class SecurityViewModel : AsyncViewModel
{
    [ObservableProperty]
    private string trusted = "N/A";

    [ObservableProperty]
    private string warnings = "N/A";

    [ObservableProperty]
    private string blocked = "N/A";

    [ObservableProperty]
    private string verified = "N/A";

    public string StatusTitle => "No verification data";

    public string StatusDescription =>
        "WinGetX does not currently have a real package-signature/security scanner connected. " +
        "Values stay N/A until a verification service is available.";

    [RelayCommand]
    public Task Refresh()
    {
        Trusted = "N/A";
        Warnings = "N/A";
        Blocked = "N/A";
        Verified = "N/A";

        return Task.CompletedTask;
    }

    public override Task InitializeAsync()
    {
        return Refresh();
    }
}
