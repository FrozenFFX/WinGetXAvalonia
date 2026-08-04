// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.ViewModels;

public sealed class HistoryGroupViewModel
{
    public string Title { get; init; } = "";

    public IReadOnlyList<HistoryItemViewModel> Items { get; init; } = [];

    public int Count => Items.Count;
}
