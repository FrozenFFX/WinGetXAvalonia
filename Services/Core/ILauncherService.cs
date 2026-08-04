namespace WinGetXAvalonia.Services.Core;

public interface ILauncherService
{
    Task OpenUrlAsync(string url);

    Task OpenFolderAsync(string path);

    Task OpenFileAsync(string path);
}
