// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

namespace WinGetXAvalonia.Services.Notifications;

public sealed class NotificationModel
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public NotificationType Type { get; init; }

    public string Title { get; init; } = "";

    public string Message { get; init; } = "";

    public DateTime Timestamp { get; init; } = DateTime.Now;

    public bool IsRead { get; set; }
}
