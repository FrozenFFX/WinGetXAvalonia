// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.ComponentModel;

namespace WinGetXAvalonia.Interfaces;

// kaneki
public interface ITimeService : INotifyPropertyChanged, IDisposable
{
    string Greeting { get; }

    string CurrentTime { get; }

    string CurrentDate { get; }

    void Start();

    void Stop();
}
// kaneki
