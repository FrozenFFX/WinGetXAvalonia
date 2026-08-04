using Avalonia.Platform.Storage;

namespace WinGetXAvalonia.Services.Dialogs;

public interface IFilePickerService
{
    Task<IStorageFile?> PickOpenJsonAsync(
        string title,
        CancellationToken cancellationToken = default);

    Task<IStorageFile?> PickSaveJsonAsync(
        string title,
        string suggestedFileName,
        CancellationToken cancellationToken = default);

    Task<IStorageFolder?> PickFolderAsync(
        string title,
        CancellationToken cancellationToken = default);
}
