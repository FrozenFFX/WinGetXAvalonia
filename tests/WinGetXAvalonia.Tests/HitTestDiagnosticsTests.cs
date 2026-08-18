// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public class HitTestDiagnosticsTests
{
    [Fact]
    public async Task ReportBlockedTextboxes()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var report = new List<string>();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var window = new MainWindow
            {
                DataContext = vm,
                Width = 1600,
                Height = 900
            };

            window.Show();
            Dispatcher.UIThread.RunJobs();

            var boxes = window.GetVisualDescendants()
                .OfType<TextBox>()
                .ToList();

            foreach (var box in boxes)
            {
                var center = box.TranslatePoint(new Point(box.Bounds.Width / 2, box.Bounds.Height / 2), window);

                if (center is null)
                {
                    report.Add($"TextBox '{box.Name ?? "?"}' ({box.GetType().Name}): translate edilemedi");
                    continue;
                }

                var hit = window.InputHitTest(center.Value);

                var hitText = hit?.GetType().Name ?? "null";
                var inside = IsInside(box, hit);

                report.Add(
                    $"TextBox '{box.Name ?? "?"}' at {center.Value.X:0},{center.Value.Y:0} " +
                    $"-> topmost hit: {hitText} {(inside ? "[TextBox]" : "[BLOCKED]")}");
            }
        });

        var blocked = report.Where(r => r.Contains("[BLOCKED]")).ToList();

        foreach (var line in report)
            Console.WriteLine(line);

        Assert.True(blocked.Count == 0,
            $"Bloklanan TextBox'lar ({blocked.Count}):" + Environment.NewLine +
            string.Join(Environment.NewLine, blocked));
    }

    private static bool IsInside(TextBox box, IInputElement? hit)
    {
        for (var current = hit as Visual; current is not null; current = current.GetVisualParent())
        {
            if (ReferenceEquals(current, box))
                return true;
        }

        return false;
    }
}
