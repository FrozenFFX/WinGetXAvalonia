namespace WinGetXAvalonia.ViewModels;

public sealed class HistoryGroupViewModel
{
    public string Title { get; init; } = "";

    public IReadOnlyList<HistoryItemViewModel> Items { get; init; } = [];

    public int Count => Items.Count;
}
