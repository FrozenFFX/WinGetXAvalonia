// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.ComponentModel;

namespace WinGetXAvalonia.Interfaces;

// kaneki
public interface ISystemMonitorService : INotifyPropertyChanged, IDisposable
{
    double CpuUsage { get; }

    double RamUsage { get; }

    double DiskUsage { get; }

    double NetworkUsage { get; }

    void Start();

    void Stop();
}
// kaneki
