using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace WinGetXAvalonia.Controls.Shared;

/// <summary>
/// Scrolls an <see cref="ItemsControl"/> back to the top whenever its
/// items are reset, e.g. after a new search replaces the results.
/// </summary>
public sealed class ScrollToTopBehavior
{
    private sealed class State
    {
        public NotifyCollectionChangedEventHandler? ItemsChanged;
    }

    private static readonly ConditionalWeakTable<ItemsControl, State> States = new();

    public static readonly AttachedProperty<bool> ScrollToTopOnResetProperty =
        AvaloniaProperty.RegisterAttached<ScrollToTopBehavior, ItemsControl, bool>(
            "ScrollToTopOnReset");

    static ScrollToTopBehavior()
    {
        ScrollToTopOnResetProperty.Changed.AddClassHandler<ItemsControl>((element, e) =>
        {
            var state = States.GetOrCreateValue(element);

            if (e.NewValue is true)
                Attach(element, state);
            else
                Detach(element, state);
        });
    }

    public static void SetScrollToTopOnReset(ItemsControl element, bool value)
        => element.SetValue(ScrollToTopOnResetProperty, value);

    public static bool GetScrollToTopOnReset(ItemsControl element)
        => element.GetValue(ScrollToTopOnResetProperty);

    private static void Attach(ItemsControl element, State state)
    {
        if (state.ItemsChanged is not null)
            return;

        state.ItemsChanged = (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                ScrollToTop(element);
        };

        if (element.Items is INotifyCollectionChanged items)
            items.CollectionChanged += state.ItemsChanged;
    }

    private static void Detach(ItemsControl element, State state)
    {
        if (state.ItemsChanged is null)
            return;

        if (element.Items is INotifyCollectionChanged items)
            items.CollectionChanged -= state.ItemsChanged;

        state.ItemsChanged = null;
    }

    private static void ScrollToTop(ItemsControl element)
        => element.FindDescendantOfType<ScrollViewer>()?.ScrollToHome();
}
