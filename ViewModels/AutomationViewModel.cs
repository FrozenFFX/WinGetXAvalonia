// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class AutomationViewModel : AsyncViewModel
{
    public ObservableCollection<AutomationItem> Automations { get; } = new();

    [ObservableProperty]
    private AutomationItem? selectedAutomation;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string trigger = "";

    [ObservableProperty]
    private string condition = "";

    [ObservableProperty]
    private string action = "";

    [RelayCommand]
    private void New()
    {
        SelectedAutomation = null;

        Name = "";
        Trigger = "";
        Condition = "";
        Action = "";
    }

    [RelayCommand]
    private void Save()
    {
    }

    [RelayCommand]
    private void Delete()
    {
    }

    [RelayCommand]
    public Task Refresh()
    {
        return Task.CompletedTask;
    }

    public Task InitializeAsync()
    {
        return Refresh();
    }
}
