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
    private SystemInformation _systemInfo = SystemInformation.Empty;

    [ObservableProperty]
    private DiagnosticsReport? report;

    public SystemInformation SystemInfo
    {
        get => _systemInfo;
        set
        {
            if (_systemInfo != value)
            {
                _systemInfo = value;
                OnPropertyChanged();
            }
        }
    }

    public DiagnosticsViewModel(IDiagnosticsService service, SystemInformation systemInfo)
    {
        _service = service;
        _systemInfo = systemInfo;
    }

    [RelayCommand]
    public async Task Refresh()
    {
        State = LoadState.Loading;

        try
        {
            Report = await _service.CreateReportAsync();
            // Populate SystemInfo from the report data
            SystemInfo = new SystemInformation
            {
                OperatingSystem = Report?.WindowsVersion != null ? "Windows" : null,
                WindowsVersion = Report?.WindowsVersion,
                Architecture = Report?.Architecture,
                MachineName = Report?.MachineName,
                UserName = Report?.UserName,
                CpuName = Report?.CpuName,
                CpuThreads = Report?.ProcessorCount,
                RamTotal = TryParseRamReport(Report?.Ram, 0),
                RamUsed = TryParseRamReport(Report?.Ram, 1),
                Disk = "Not implemented",
                GpuName = "Not implemented",
                Network = "Not implemented",
                DotNetVersion = Environment.Version.ToString(),
                WingetVersion = "N/A"
            };
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

    private static double? TryParseRamReport(string? ram, int index)
    {
        if (string.IsNullOrWhiteSpace(ram))
            return null;

        var parts = ram.Split(" / ");
        if (parts.Length <= index)
            return null;

        return double.TryParse(parts[index].Trim(), out var value) ? value : null;
    }
}
