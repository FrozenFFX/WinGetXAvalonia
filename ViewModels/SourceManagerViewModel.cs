// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.Sources;

namespace WinGetXAvalonia.ViewModels;

public partial class SourceManagerViewModel : AsyncViewModel
{
    private readonly ISourceManagerService _service;
    private readonly IDialogService _dialogService;

    public ObservableCollection<WingetSource> Sources { get; } = new();

    [ObservableProperty]
    private WingetSource? selectedSource;

    public SourceManagerViewModel(ISourceManagerService service, IDialogService dialogService)
    {
        _service = service;
        _dialogService = dialogService;
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
    {
        return _service.ResetAsync();
    }

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
