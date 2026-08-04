// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

using System.Collections.ObjectModel;

namespace WinGetXAvalonia.Services.Notifications;

public sealed class NotificationService
{
    private const int MaxNotifications = 100;

    private readonly ObservableCollection<NotificationModel> _items = new();

    public event EventHandler? NotificationsChanged;

    public IReadOnlyList<NotificationModel> Notifications => _items;

    public int UnreadCount
    {
        get
        {
            int count = 0;

            foreach (var item in _items)
            {
                if (!item.IsRead)
                    count++;
            }

            return count;
        }
    }

    public void Notify(NotificationType type, string title, string? message = null)
    {
        _items.Insert(0, new NotificationModel
        {
            Type = type,
            Title = title,
            Message = message ?? ""
        });

        while (_items.Count > MaxNotifications)
            _items.RemoveAt(_items.Count - 1);

        NotificationsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void MarkAllRead()
    {
        foreach (var item in _items)
            item.IsRead = true;

        NotificationsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _items.Clear();

        NotificationsChanged?.Invoke(this, EventArgs.Empty);
    }
}
