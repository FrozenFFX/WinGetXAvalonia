// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Sources;

namespace WinGetXAvalonia.ViewModels;

public partial class SourceManagerViewModel : AsyncViewModel
{
    private readonly ISourceManagerService _service;

    public ObservableCollection<WingetSource> Sources { get; } = new();

    [ObservableProperty]
    private WingetSource? selectedSource;

    public SourceManagerViewModel(ISourceManagerService service)
    {
        _service = service;
    }

    [RelayCommand]
    public async Task Refresh()
    {
        State = LoadState.Loading;

        try
        {
            Sources.Clear();

            foreach (var source in await _service.GetSourcesAsync())
                Sources.Add(source);

            State = Sources.Count > 0 ? LoadState.Loaded : LoadState.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = LoadState.Error;
        }
    }

    [RelayCommand]
    private Task Update()
        => _service.UpdateAsync();

    [RelayCommand]
    private Task Reset()
        => _service.ResetAsync();

    [RelayCommand]
    private Task Remove()
    {
        if (SelectedSource == null)
            return Task.CompletedTask;

        return _service.RemoveAsync(SelectedSource.Name);
    }

    public override Task InitializeAsync()
    {
        return Refresh();
    }
}
