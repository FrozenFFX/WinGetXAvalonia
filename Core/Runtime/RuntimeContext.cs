using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinGetXAvalonia.Core.Runtime;

public sealed class RuntimeContext : INotifyPropertyChanged
{
    private RuntimeState _state = RuntimeState.Starting;

    public DateTime StartedAt { get; } = DateTime.Now;

    public TimeSpan Uptime => DateTime.Now - StartedAt;

    public RuntimeState State
    {
        get => _state;
        set
        {
            if (_state == value)
                return;

            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsReady));
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(StateText));
        }
    }

    public bool IsReady => State == RuntimeState.Ready;

    public bool IsBusy => State == RuntimeState.Busy;

    public string StateText => State.ToString();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
