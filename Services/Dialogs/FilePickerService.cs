using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace WinGetXAvalonia.Services.Dialogs;

public sealed class FilePickerService : IFilePickerService
{
    private static readonly FilePickerFileType JsonFileType =
        new("JSON file") { Patterns = new[] { "*.json" } };

    public async Task<IStorageFile?> PickOpenJsonAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var window = GetMainWindow();

        if (window is null)
            return null;

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new[] { JsonFileType }
        });

        cancellationToken.ThrowIfCancellationRequested();
        return files.FirstOrDefault();
    }

    public async Task<IStorageFile?> PickSaveJsonAsync(
        string title,
        string suggestedFileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var window = GetMainWindow();

        if (window is null)
            return null;

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            DefaultExtension = "json",
            FileTypeChoices = new[] { JsonFileType }
        });

        cancellationToken.ThrowIfCancellationRequested();
        return file;
    }

    public async Task<IStorageFolder?> PickFolderAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var window = GetMainWindow();

        if (window is null)
            return null;

        var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        cancellationToken.ThrowIfCancellationRequested();
        return folders.FirstOrDefault();
    }

    private static Window? GetMainWindow()
        => Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }
            ? window
            : null;
}
