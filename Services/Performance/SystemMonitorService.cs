// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using WinGetXAvalonia.Interfaces;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Services.Performance;

public sealed class SystemMonitorService : ISystemMonitorService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(2));
    private readonly CancellationTokenSource _cts = new();
    private readonly SystemInfoService _system;
    private readonly LoggerService _logger;

    private Task? _loop;

    private ulong _prevIdle;
    private ulong _prevTotal;
    private bool _cpuSampled;

    private long _prevBytes;
    private long _prevTick;
    private bool _netSampled;

    private double _cpuUsage;
    private double _ramUsage;
    private double _diskUsage;
    private double _networkUsage;

    public double CpuUsage => _cpuUsage;

    public double RamUsage => _ramUsage;

    public double DiskUsage => _diskUsage;

    public double NetworkUsage => _networkUsage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public SystemMonitorService(SystemInfoService system, LoggerService logger)
    {
        _system = system;
        _logger = logger;
    }

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
                try
                {
                    UpdateMetrics();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "System monitor tick failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // monitor stopped
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "System monitor loop failed");
        }
    }

    private void UpdateMetrics()
    {
        SetValue(ReadCpuUsage(), ref _cpuUsage, nameof(CpuUsage));
        SetValue(ReadRamUsage(), ref _ramUsage, nameof(RamUsage));
        SetValue(ReadDiskUsage(), ref _diskUsage, nameof(DiskUsage));
        SetValue(ReadNetworkUsage(), ref _networkUsage, nameof(NetworkUsage));
    }

    private double ReadCpuUsage()
    {
        FILETIME idle;
        FILETIME kernel;
        FILETIME user;

        try
        {
            if (!GetSystemTimes(out idle, out kernel, out user))
                return _cpuUsage;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read CPU usage");
            return _cpuUsage;
        }

        ulong idleTime = ToUInt64(idle);
        ulong totalTime = ToUInt64(kernel) + ToUInt64(user);

        if (!_cpuSampled)
        {
            _prevIdle = idleTime;
            _prevTotal = totalTime;
            _cpuSampled = true;
            return _cpuUsage;
        }

        ulong idleDelta = idleTime - _prevIdle;
        ulong totalDelta = totalTime - _prevTotal;

        _prevIdle = idleTime;
        _prevTotal = totalTime;

        if (totalDelta == 0)
            return _cpuUsage;

        double usage = 100.0 * (1.0 - (double)idleDelta / totalDelta);
        return Math.Clamp(usage, 0, 100);
    }

    private double ReadRamUsage()
    {
        try
        {
            return _system.GetRamUsage();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read RAM usage");
            return _ramUsage;
        }
    }

    private double ReadDiskUsage()
    {
        try
        {
            string? root = Path.GetPathRoot(Environment.SystemDirectory);

            if (root is null)
                return 0;

            var drive = new DriveInfo(root);

            if (!drive.IsReady || drive.TotalSize == 0)
                return 0;

            double used = drive.TotalSize - drive.AvailableFreeSpace;
            return used / drive.TotalSize * 100;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read disk usage");
            return _diskUsage;
        }
    }

    private double ReadNetworkUsage()
    {
        long total = 0;

        try
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
                    continue;

                var stats = nic.GetIPv4Statistics();
                total += stats.BytesReceived;
                total += stats.BytesSent;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read network usage");
            return _networkUsage;
        }

        long now = Environment.TickCount64;

        if (!_netSampled)
        {
            _prevBytes = total;
            _prevTick = now;
            _netSampled = true;
            return _networkUsage;
        }

        double elapsed = (now - _prevTick) / 1000.0;
        double delta = total - _prevBytes;

        _prevBytes = total;
        _prevTick = now;

        if (elapsed <= 0)
            return _networkUsage;

        return Math.Max(0, delta / elapsed);
    }

    private void SetValue(
        double value,
        ref double field,
        [CallerMemberName] string? name = null)
    {
        field = Math.Round(value, 1);

        if (PropertyChanged is not { } handler)
            return;

        Dispatcher.UIThread.Post(() =>
            handler(this, new PropertyChangedEventArgs(name)));
    }

    private static ulong ToUInt64(FILETIME ft)
        => ((ulong)ft.dwHighDateTime << 32) | ft.dwLowDateTime;

    [StructLayout(LayoutKind.Sequential)]
    private struct FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }

    [DllImport("kernel32.dll")]
    private static extern bool GetSystemTimes(
        out FILETIME lpIdleTime,
        out FILETIME lpKernelTime,
        out FILETIME lpUserTime);
}
