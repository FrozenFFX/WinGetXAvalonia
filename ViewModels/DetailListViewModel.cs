// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class DetailListViewModel : ViewModelBase
{
    public ObservableCollection<HeroInfoItem> Items { get; } = new();
}
