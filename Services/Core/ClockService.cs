// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Threading;

namespace WinGetXAvalonia.Services.Core;

public class ClockService : IDisposable
{
    private readonly DispatcherTimer _timer;
    private string _lastDate;

    public event Action<string>? Tick;
    public event Action<string>? DateChanged;

    public string CurrentTime => DateTime.Now.ToString("HH:mm");

    public string CurrentDate => DateTime.Now.ToString("dd MMM yyyy");

    public ClockService()
    {
        _lastDate = CurrentDate;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        Tick?.Invoke(CurrentTime);

        var date = CurrentDate;
        if (date == _lastDate)
            return;

        _lastDate = date;
        DateChanged?.Invoke(date);
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }
}
