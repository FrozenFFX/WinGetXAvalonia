// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Core.State;

// kaneki
public abstract partial class AsyncViewModel : ViewModelBase
{
    [ObservableProperty]
    private LoadState state = LoadState.None;

    [ObservableProperty]
    private string? errorMessage;

    public bool IsLoading => State == LoadState.Loading;

    public bool IsEmpty => State == LoadState.Empty;

    public bool HasError => State == LoadState.Error;

    public bool IsLoaded => State == LoadState.Loaded;
}
// kaneki
