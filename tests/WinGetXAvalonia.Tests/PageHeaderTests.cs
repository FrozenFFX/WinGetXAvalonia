// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.Controls.Cards;
using WinGetXAvalonia.Controls.Shared;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public class PageHeaderTests
{
    [Fact]
    public async Task WithoutActionCommand_ActionButtonIsHidden()
    {
        using var host = Program.CreateHost();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var header = new PageHeader
            {
                Title = "Test",
                Description = "Açıklama"
            };

            var window = new Window { Content = header };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var button = FindActionButton(header);

            Assert.NotNull(button);
            Assert.False(button.IsEffectivelyVisible);
            AssertHeaderText(header, "Test", "Açıklama");
            Assert.False(header.GetVisualDescendants().OfType<StackPanel>()
                .First(p => p.Orientation == Avalonia.Layout.Orientation.Horizontal
                            && Grid.GetColumn(p) == 1).IsVisible);

            window.Close();
        });
    }

    [Fact]
    public async Task WithActionCommand_ActionButtonIsVisible()
    {
        using var host = Program.CreateHost();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var header = new PageHeader
            {
                Title = "Test",
                Description = "Açıklama",
                ActionText = "Retry",
                ActionCommand = new FakeCommand()
            };

            var window = new Window { Content = header };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var button = FindActionButton(header);

            Assert.NotNull(button);
            Assert.True(button.IsEffectivelyVisible);
            AssertHeaderText(header, "Test", "Açıklama");
            Assert.Equal("Retry", button.Content);

            window.Close();
        });
    }

    [Fact]
    public async Task Cards_RenderLocalizedControlProperties()
    {
        using var host = Program.CreateHost();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var actionCard = new ActionCard
            {
                Title = "Installed",
                Description = "Manage packages"
            };
            var statCard = new StatCard
            {
                Value = "223",
                Subtitle = "Installed",
                Secondary = "4 favorites"
            };
            var content = new StackPanel
            {
                Children = { actionCard, statCard }
            };
            var window = new Window { Content = content };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var actionText = string.Join(
                "\n",
                actionCard.GetVisualDescendants().OfType<TextBlock>().Select(block => block.Text));
            var statText = string.Join(
                "\n",
                statCard.GetVisualDescendants().OfType<TextBlock>().Select(block => block.Text));

            Assert.Contains("Installed", actionText);
            Assert.Contains("Manage packages", actionText);
            Assert.Contains("223", statText);
            Assert.Contains("Installed", statText);
            Assert.Contains("4 favorites", statText);

            window.Close();
        });
    }

    private static void AssertHeaderText(PageHeader header, string title, string description)
    {
        var text = string.Join(
            "\n",
            header.GetVisualDescendants().OfType<TextBlock>().Select(block => block.Text));

        Assert.Contains(title, text);
        Assert.Contains(description, text);
    }

    private static Button? FindActionButton(PageHeader header)
        => header.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(b => ReferenceEquals(b.Command, header.ActionCommand)
                                 || b.Parent is StackPanel { Orientation: Avalonia.Layout.Orientation.Horizontal } sp
                                 && Grid.GetColumn(sp) == 1);

    private sealed class FakeCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }
    }
}
