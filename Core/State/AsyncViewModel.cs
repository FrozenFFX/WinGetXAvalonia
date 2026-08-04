// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Core.State;

public abstract partial class AsyncViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsLoaded))]
    private LoadState state = LoadState.None;

    [ObservableProperty]
    private string? errorMessage;

    public bool IsLoading => State == LoadState.Loading;

    public bool IsEmpty => State == LoadState.Empty;

    public bool HasError => State == LoadState.Error;

    public bool IsLoaded => State == LoadState.Loaded;
}
