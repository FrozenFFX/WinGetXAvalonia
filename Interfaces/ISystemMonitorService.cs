// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.ComponentModel;

namespace WinGetXAvalonia.Interfaces;

public interface ISystemMonitorService : INotifyPropertyChanged, IDisposable
{
    double CpuUsage { get; }

    double RamUsage { get; }

    double DiskUsage { get; }

    double NetworkUsage { get; }

    void Start();

    void Stop();
}
