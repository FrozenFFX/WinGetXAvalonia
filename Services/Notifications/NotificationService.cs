// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace WinGetXAvalonia.Services.Notifications;

public sealed class NotificationService : INotificationService
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
        var item = new NotificationModel
        {
            Type = type,
            Title = title,
            Message = message ?? ""
        };

        Add(item);
    }

    public void ShowSuccess(string title, string message)
        => Notify(NotificationType.Success, title, message);

    public void ShowError(string title, string message)
        => Notify(NotificationType.Error, title, message);

    public void ShowWarning(string title, string message)
        => Notify(NotificationType.Warning, title, message);

    public void ShowInfo(string title, string message)
        => Notify(NotificationType.Information, title, message);

    private void Add(NotificationModel item)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            AddCore(item);
            return;
        }

        Dispatcher.UIThread.Invoke(() => AddCore(item));
    }

    private void AddCore(NotificationModel item)
    {
        _items.Insert(0, item);

        while (_items.Count > MaxNotifications)
            _items.RemoveAt(_items.Count - 1);

        NotificationsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Success(string message)
        => Notify(NotificationType.Success, message);

    public void Warning(string message)
        => Notify(NotificationType.Warning, message);

    public void Error(string message)
        => Notify(NotificationType.Error, message);

    public void Info(string message)
        => Notify(NotificationType.Information, message);

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
