using Avalonia.Threading;

namespace WinGetXAvalonia.Services.Core;

public class ClockService : IDisposable
{
    private readonly DispatcherTimer _timer;

    public event Action<string>? Tick;

    public string CurrentTime => DateTime.Now.ToString("HH:mm");

    public string CurrentDate => DateTime.Now.ToString("dd MMM yyyy");

    public ClockService()
    {
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
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }
}
