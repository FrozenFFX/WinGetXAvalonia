// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
namespace WinGetXAvalonia.Services.Notifications;

// kaneki
public interface INotificationService
{
    void ShowSuccess(string title, string message);

    void ShowError(string title, string message);

    void ShowWarning(string title, string message);

    void ShowInfo(string title, string message);
}
// kaneki
