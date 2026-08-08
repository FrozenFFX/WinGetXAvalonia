// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Core;

/// <summary>
/// Fire-and-forget görevlerde unobserved exception'ların uygulamayı
/// çökertmesini engeller. Hata alınırsa log'a yazılır, uygulama devam eder.
/// </summary>
public static class TaskExtensions
{
    public static void FireAndForget(
        this Task task,
        LoggerService? logger = null,
        string? context = null)
    {
        _ = task.ContinueWith(
            t =>
            {
                try
                {
                    if (t.Exception is { } ex)
                    {
                        if (logger is not null)
                            logger.Error(ex, context ?? "Unhandled exception in fire-and-forget task");
                        else
                            System.Diagnostics.Debug.WriteLine(
                                $"Unhandled task exception: {ex}");
                    }
                }
                catch (Exception logException)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to log task exception: {logException}");
                }
            },
            TaskContinuationOptions.OnlyOnFaulted);
    }
}
