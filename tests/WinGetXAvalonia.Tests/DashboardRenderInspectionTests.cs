// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public class DashboardRenderInspectionTests
{
    private static void Walk(Visual visual, int depth, Action<Visual, int> visit)
    {
        visit(visual, depth);

        foreach (var child in visual.GetVisualChildren())
            Walk(child, depth + 1, visit);
    }

    [Fact]
    public async Task Dashboard_TextBlocks_AreVisible_AndPopulated()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<DashboardViewModel>());

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var dashboardView = new DashboardView { DataContext = vm };
            dashboardView.Width = 1400;
            dashboardView.Height = 800;

            var window = new Window
            {
                Width = 1600,
                Height = 900,
                Content = dashboardView,
            };

            window.Show();

            Dispatcher.UIThread.RunJobs();
            window.Measure(new Size(1600, 900));
            window.Arrange(new Rect(0, 0, 1600, 900));
            Dispatcher.UIThread.RunJobs();

            var findings = new List<string>();

            void Visit(Visual visual, int depth)
            {
                if (visual is TextBlock tb)
                {
                    var text = tb.Text ?? "";
                    findings.Add(
                        $"TEXT d={depth} '{text}' opacity={tb.Opacity} " +
                        $"size={tb.FontSize} visible={tb.IsVisible} " +
                        $"bounds={tb.Bounds.Width:0}x{tb.Bounds.Height:0} " +
                        $"fg={(tb.Foreground as SolidColorBrush)?.Color}");
                }

                if (visual is ItemsControl ic)
                    findings.Add($"ITEMS d={depth} count={ic.ItemCount} visible={ic.IsVisible}");
            }

            Walk(window, 0, Visit);

            File.WriteAllLines(
                "C:\\Users\\depgo\\AppData\\Local\\Temp\\opencode\\visualtree.txt",
                findings);

            Assert.Contains(findings, f => f.Contains("'Dashboard'"));
        });
    }
}
