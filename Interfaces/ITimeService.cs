// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.ComponentModel;

namespace WinGetXAvalonia.Interfaces;

public interface ITimeService : INotifyPropertyChanged, IDisposable
{
    string Greeting { get; }

    string CurrentTime { get; }

    string CurrentDate { get; }

    void Start();

    void Stop();
}
