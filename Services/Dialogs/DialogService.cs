using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace WinGetXAvalonia.Services.Dialogs;

public sealed class DialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null)
    {
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.UIThread.Post(() =>
        {
            var owner = OwnerWindow();
            var window = CreateWindow(title);

            bool result = false;

            var cancel = new Button { Content = cancelText ?? "Cancel" };
            var confirm = new Button
            {
                Content = confirmText ?? "Confirm",
                Classes = { "Danger" }
            };

            cancel.Click += (_, _) =>
            {
                result = false;
                window.Close();
            };

            confirm.Click += (_, _) =>
            {
                result = true;
                window.Close();
            };

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                Children = { cancel, confirm }
            };

            window.Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14
                    },
                    buttons
                }
            };

            if (owner is not null)
            {
                _ = window.ShowDialog<bool>(owner).ContinueWith(t =>
                    tcs.TrySetResult(t.Result == true), TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                window.Show();
                window.Closed += (_, _) => tcs.TrySetResult(result);
            }
        });

        return tcs.Task;
    }

    public Task ProgressAsync(string title, Func<IProgress<double>, Task> operation)
    {
        var tcs = new TaskCompletionSource();

        Dispatcher.UIThread.Post(async () =>
        {
            var owner = OwnerWindow();
            var window = CreateWindow(title);

            window.Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    new ProgressBar
                    {
                        Width = 320,
                        IsIndeterminate = true
                    },
                    new TextBlock
                    {
                        Text = "Please wait...",
                        Opacity = 0.7,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }
            };

            if (owner is not null)
                window.Show(owner);
            else
                window.Show();

            try
            {
                await operation(new Progress<double>());
                window.Close();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                window.Close();
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    public Task ShowErrorAsync(string title, string message)
    {
        var tcs = new TaskCompletionSource();

        Dispatcher.UIThread.Post(() =>
        {
            var owner = OwnerWindow();
            var window = CreateWindow(title);

            var ok = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Right
            };

            ok.Click += (_, _) => window.Close();

            window.Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14
                    },
                    ok
                }
            };

            if (owner is not null)
            {
                window.ShowDialog(owner);
            }
            else
            {
                window.Show();
            }

            window.Closed += (_, _) => tcs.TrySetResult();
        });

        return tcs.Task;
    }

    private static Window CreateWindow(string title) => new()
    {
        Title = title,
        Width = 440,
        CanResize = false,
        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        Background = AppBrush("CardBrush")
    };

    private static Window? OwnerWindow()
        => Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }
            ? window
            : null;

    private static IBrush? AppBrush(string key)
    {
        if (Application.Current is { } app
            && app.TryFindResource(key, out var value)
            && value is IBrush brush)
            return brush;

        return null;
    }
}
