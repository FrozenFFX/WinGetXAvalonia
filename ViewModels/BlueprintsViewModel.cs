// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class BlueprintsViewModel : AsyncViewModel
{
    public ObservableCollection<BlueprintModel> Blueprints { get; } = new();

    [ObservableProperty]
    private BlueprintModel? selectedBlueprint;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string description = "";

    [RelayCommand]
    private void New()
    {
        SelectedBlueprint = null;

        Name = "";
        Description = "";
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
