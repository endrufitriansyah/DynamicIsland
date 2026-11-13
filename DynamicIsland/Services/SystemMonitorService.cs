using System;
using System.Runtime.InteropServices;
using System.Timers;
using DynamicIsland.Helpers;

namespace DynamicIsland.Services;

public class SystemMetrics
{
    public double CpuUsagePercent { get; set; }
    public double RamUsedGb { get; set; }
    public double RamTotalGb { get; set; }
    public double RamUsagePercent { get; set; }
}

public class SystemMonitorService : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private ulong _previousIdleTime;
    private ulong _previousKernelTime;
    private ulong _previousUserTime;
    private bool _isFirstReading = true;
    private bool _isDisposed;

    public event Action<SystemMetrics>? MetricsUpdated;
    public SystemMetrics CurrentMetrics { get; private set; } = new();

    public SystemMonitorService(double intervalMs = 1500)
    {
        _timer = new System.Timers.Timer(intervalMs);
        _timer.Elapsed += (s, e) => PollMetrics();
    }

    public void Start()
    {
        PollMetrics();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private void PollMetrics()
    {
        double cpu = CalculateCpuUsage();
        var (ramUsed, ramTotal, ramPercent) = CalculateRamUsage();

        CurrentMetrics = new SystemMetrics
        {
            CpuUsagePercent = Math.Round(cpu, 0),
            RamUsedGb = Math.Round(ramUsed, 1),
            RamTotalGb = Math.Round(ramTotal, 1),
            RamUsagePercent = Math.Round(ramPercent, 0)
        };

        MetricsUpdated?.Invoke(CurrentMetrics);
    }

    private double CalculateCpuUsage()
    {
        if (!NativeMethods.GetSystemTimes(out var idleTime, out var kernelTime, out var userTime))
        {
            return 0;
        }

        ulong idle = idleTime.ToUInt64();
        ulong kernel = kernelTime.ToUInt64();
        ulong user = userTime.ToUInt64();

        if (_isFirstReading)
        {
            _previousIdleTime = idle;
            _previousKernelTime = kernel;
            _previousUserTime = user;
            _isFirstReading = false;
            return 0;
        }

        ulong usrDiff = user - _previousUserTime;
        ulong kerDiff = kernel - _previousKernelTime;
        ulong idlDiff = idle - _previousIdleTime;

        _previousIdleTime = idle;
        _previousKernelTime = kernel;
        _previousUserTime = user;

        ulong totalSys = usrDiff + kerDiff;
        if (totalSys == 0) return 0;

        // Note: Kernel time includes idle time in Windows NT
        ulong activeSys = totalSys > idlDiff ? totalSys - idlDiff : 0;
        double cpuPercentage = (double)(activeSys * 100) / totalSys;

        return Math.Clamp(cpuPercentage, 0.0, 100.0);
    }

    private static (double usedGb, double totalGb, double percent) CalculateRamUsage()
    {
        var memStatus = new NativeMethods.MEMORYSTATUSEX
        {
            dwLength = (uint)Marshal.SizeOf<NativeMethods.MEMORYSTATUSEX>()
        };

        if (NativeMethods.GlobalMemoryStatusEx(ref memStatus))
        {
            double totalGb = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
            double availGb = memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
            double usedGb = totalGb - availGb;
            double percent = memStatus.dwMemoryLoad;

            return (usedGb, totalGb, percent);
        }

        return (0, 0, 0);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _timer.Stop();
        _timer.Dispose();
    }
}
