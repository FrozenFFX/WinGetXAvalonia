// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Diagnostics;

namespace WinGetXAvalonia.ViewModels;

public partial class DiagnosticsViewModel : AsyncViewModel
{
    private readonly IDiagnosticsService _service;

    [ObservableProperty]
    private DiagnosticsReport? report;

    public DiagnosticsViewModel(IDiagnosticsService service)
    {
        _service = service;
    }

    [RelayCommand]
    public async Task Refresh()
    {
        State = LoadState.Loading;

        try
        {
            Report = await _service.CreateReportAsync();
            State = LoadState.Loaded;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = LoadState.Error;
        }
    }

    public override Task InitializeAsync()
    {
        return Refresh();
    }
}
