using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public sealed class HistoryItemViewModel
{
    public HistoryEntry Entry { get; }

    public HistoryItemViewModel(HistoryEntry entry)
    {
        Entry = entry;
    }

    public string Icon => Entry.Success ? "✓" : "✕";

    public string Title => Entry.Success
        ? SuccessTitle(Entry.Operation)
        : $"{Normalize(Entry.Operation)} Failed";

    public string PackageName =>
        string.IsNullOrWhiteSpace(Entry.PackageName) ? Entry.PackageId : Entry.PackageName;

    public string Time => Entry.Timestamp.ToString("HH:mm");

    public string Status => Entry.Success
        ? "Success"
        : string.IsNullOrWhiteSpace(Entry.Message) ? "Failed" : Entry.Message;

    public string PackageId => Entry.PackageId;

    public string Operation => Entry.Operation;

    public bool IsSuccess => Entry.Success;

    public bool IsOperation(string operation)
        => Entry.Operation.Equals(operation, StringComparison.OrdinalIgnoreCase);

    private static string SuccessTitle(string operation) => Normalize(operation) switch
    {
        "Install" => "Installed",
        "Update" => "Updated",
        "Remove" => "Removed",
        var other => other
    };

    private static string Normalize(string operation) => operation.Trim() switch
    {
        var value when value.Equals("Install", StringComparison.OrdinalIgnoreCase) => "Install",
        var value when value.Equals("Update", StringComparison.OrdinalIgnoreCase) => "Update",
        var value when value.Equals("Remove", StringComparison.OrdinalIgnoreCase) => "Remove",
        var value => value
    };
}
