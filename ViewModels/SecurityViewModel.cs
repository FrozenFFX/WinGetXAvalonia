// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;

namespace WinGetXAvalonia.ViewModels;

public partial class SecurityViewModel : AsyncViewModel
{
    [ObservableProperty]
    private int trusted;

    [ObservableProperty]
    private int warnings;

    [ObservableProperty]
    private int blocked;

    [ObservableProperty]
    private int verified;

    [RelayCommand]
    public Task Refresh()
    {
        Trusted = 0;
        Warnings = 0;
        Blocked = 0;
        Verified = 0;

        return Task.CompletedTask;
    }

    public override Task InitializeAsync()
    {
        return Refresh();
    }
}
