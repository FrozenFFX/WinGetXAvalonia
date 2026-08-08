// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Services.Core;

public partial class NavigationService : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase? currentPage;

    public bool Navigate(ViewModelBase page)
    {
        if (page is null)
            return false;

        CurrentPage = page;

        return true;
    }
}
