using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class NotificationServiceTests
{
    [Fact]
    public void Notify_AddsToTop_AndFiresChanged()
    {
        var service = new NotificationService();
        var changed = 0;
        service.NotificationsChanged += (_, _) => changed++;

        service.Notify(NotificationType.Success, "Git Installed");

        Assert.Equal(1, changed);

        var notification = Assert.Single(service.Notifications);
        Assert.Equal(NotificationType.Success, notification.Type);
        Assert.Equal("Git Installed", notification.Title);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public void Notify_NewestFirst()
    {
        var service = new NotificationService();

        service.Notify(NotificationType.Success, "first");
        service.Notify(NotificationType.Error, "second");

        Assert.Equal(2, service.Notifications.Count);
        Assert.Equal("second", service.Notifications[0].Title);
    }

    [Fact]
    public void UnreadCount_TracksReadState()
    {
        var service = new NotificationService();
        service.Notify(NotificationType.Success, "a");
        service.Notify(NotificationType.Warning, "b");
        service.Notify(NotificationType.Error, "c");

        Assert.Equal(3, service.UnreadCount);

        service.MarkAllRead();

        Assert.Equal(0, service.UnreadCount);
        Assert.All(service.Notifications, n => Assert.True(n.IsRead));
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var service = new NotificationService();
        service.Notify(NotificationType.Success, "a");

        service.Clear();

        Assert.Empty(service.Notifications);
        Assert.Equal(0, service.UnreadCount);
    }

    [Fact]
    public void Notify_CapsAtMaximum()
    {
        var service = new NotificationService();

        for (int i = 0; i < 150; i++)
            service.Notify(NotificationType.Success, $"n{i}");

        Assert.Equal(100, service.Notifications.Count);
        Assert.Equal("n149", service.Notifications[0].Title);
    }
}

public class NotificationCenterViewModelTests
{
    [Fact]
    public void Refresh_SyncsItemsAndUnreadCount()
    {
        var service = new NotificationService();
        service.Notify(NotificationType.Success, "Git Installed");
        service.Notify(NotificationType.Error, "Docker Failed");

        var vm = new NotificationCenterViewModel(service);

        Assert.Equal(2, vm.Items.Count);
        Assert.Equal(2, vm.UnreadCount);
        Assert.True(vm.HasUnread);
        Assert.False(vm.IsEmpty);
    }

    [Fact]
    public void NewNotification_UpdatesBadge()
    {
        var service = new NotificationService();
        var vm = new NotificationCenterViewModel(service);
        Assert.True(vm.IsEmpty);

        service.Notify(NotificationType.Success, "Git Installed");

        Assert.Single(vm.Items);
        Assert.Equal(1, vm.UnreadCount);
        Assert.True(vm.HasUnread);
        Assert.False(vm.IsEmpty);
    }

    [Fact]
    public void MarkAllRead_ClearsBadge()
    {
        var service = new NotificationService();
        service.Notify(NotificationType.Success, "Git Installed");

        var vm = new NotificationCenterViewModel(service);
        vm.MarkAllReadCommand.Execute(null);

        Assert.Equal(0, vm.UnreadCount);
        Assert.False(vm.HasUnread);
        Assert.True(vm.Items[0].IsRead);
    }

    [Fact]
    public void Clear_EmptiesList()
    {
        var service = new NotificationService();
        service.Notify(NotificationType.Success, "Git Installed");

        var vm = new NotificationCenterViewModel(service);
        vm.ClearCommand.Execute(null);

        Assert.Empty(vm.Items);
        Assert.True(vm.IsEmpty);
    }
}
