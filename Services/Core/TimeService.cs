// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.ComponentModel;
using System.Runtime.CompilerServices;
using WinGetXAvalonia.Interfaces;

namespace WinGetXAvalonia.Services.Core;

public sealed class TimeService : ITimeService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1));
    private readonly CancellationTokenSource _cts = new();

    private Task? _loop;

    private string _greeting = GetGreeting(DateTime.Now.Hour);
    private string _currentTime = DateTime.Now.ToString("HH:mm");
    private string _currentDate = DateTime.Now.ToString("dd MMMM yyyy");

    public string Greeting => _greeting;

    public string CurrentTime => _currentTime;

    public string CurrentDate => _currentDate;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Start()
    {
        if (_loop is not null)
            return;

        _loop = Task.Run(RunAsync);
    }

    public void Stop()
    {
        _cts.Cancel();
        _timer.Dispose();
    }

    public void Dispose()
    {
        Stop();
    }

    private async Task RunAsync()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                var now = DateTime.Now;
                SetValue(GetGreeting(now.Hour), ref _greeting, nameof(Greeting));
                SetValue(now.ToString("HH:mm"), ref _currentTime, nameof(CurrentTime));
                SetValue(now.ToString("dd MMMM yyyy"), ref _currentDate, nameof(CurrentDate));
            }
        }
        catch (OperationCanceledException)
        {
            // service stopped
        }
    }

    private void SetValue(
        string value,
        ref string field,
        [CallerMemberName] string? name = null)
    {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private static string GetGreeting(int hour)
        => hour switch
        {
            < 12 => "Good Morning",
            < 18 => "Good Afternoon",
            _ => "Good Evening"
        };
}
