// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Services.Notifications;

namespace WinGetXAvalonia.ViewModels;

public partial class NotificationCenterViewModel : ViewModelBase
{
    private readonly NotificationService _service;

    public ObservableCollection<NotificationModel> Items { get; } = new();

    [ObservableProperty]
    private int unreadCount;

    [ObservableProperty]
    private bool hasUnread;

    [ObservableProperty]
    private bool isEmpty = true;

    public NotificationCenterViewModel(NotificationService service)
    {
        _service = service;
        _service.NotificationsChanged += (_, _) => Refresh();
        Refresh();
    }

    public void Refresh()
    {
        var snapshot = _service.Notifications;

        foreach (var item in snapshot)
        {
            if (!Items.Contains(item))
                Items.Add(item);
        }

        for (int i = Items.Count - 1; i >= 0; i--)
        {
            if (!snapshot.Contains(Items[i]))
                Items.RemoveAt(i);
        }

        UnreadCount = _service.UnreadCount;
        HasUnread = UnreadCount > 0;
        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    private void MarkAllRead()
    {
        _service.MarkAllRead();
        Refresh();
    }

    [RelayCommand]
    private void Clear()
    {
        _service.Clear();
        Refresh();
    }
}
