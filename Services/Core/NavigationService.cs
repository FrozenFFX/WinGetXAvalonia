// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Services.Core;

public partial class NavigationService : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase? currentPage;

    public void Navigate(ViewModelBase page)
    {
        if (CurrentPage == page)
            return;

        CurrentPage = page;
    }
}
