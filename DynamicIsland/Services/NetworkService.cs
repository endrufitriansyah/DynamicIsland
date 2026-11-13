using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Timers;

namespace DynamicIsland.Services;

public class NetworkMetrics
{
    public double DownloadSpeedBytesPerSec { get; set; }
    public double UploadSpeedBytesPerSec { get; set; }
    public string DownloadSpeedFormatted { get; set; } = "0 B/s";
    public string UploadSpeedFormatted { get; set; } = "0 B/s";
    public bool IsConnected { get; set; } = true;
    public string ActiveAdapterName { get; set; } = "Ethernet / Wi-Fi";
}

public class NetworkService : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private long _lastBytesReceived;
    private long _lastBytesSent;
    private DateTime _lastPollTime = DateTime.UtcNow;
    private bool _isFirstPoll = true;
    private bool _isDisposed;

    public event Action<NetworkMetrics>? NetworkUpdated;
    public NetworkMetrics CurrentMetrics { get; private set; } = new();

    public NetworkService(double intervalMs = 1500)
    {
        _timer = new System.Timers.Timer(intervalMs);
        _timer.Elapsed += (s, e) => PollNetwork();

        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
    }

    public void Start()
    {
        PollNetwork();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
    {
        CurrentMetrics.IsConnected = e.IsAvailable;
        NetworkUpdated?.Invoke(CurrentMetrics);
    }

    private void PollNetwork()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                              nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .ToList();

            long totalReceived = 0;
            long totalSent = 0;
            string adapterName = interfaces.FirstOrDefault()?.Name ?? "Jaringan";

            foreach (var nic in interfaces)
            {
                var stats = nic.GetIPv4Statistics();
                totalReceived += stats.BytesReceived;
                totalSent += stats.BytesSent;
            }

            var now = DateTime.UtcNow;
            double seconds = (now - _lastPollTime).TotalSeconds;
            if (seconds <= 0) seconds = 1.0;

            if (_isFirstPoll)
            {
                _lastBytesReceived = totalReceived;
                _lastBytesSent = totalSent;
                _lastPollTime = now;
                _isFirstPoll = false;
                return;
            }

            long diffReceived = totalReceived >= _lastBytesReceived ? totalReceived - _lastBytesReceived : 0;
            long diffSent = totalSent >= _lastBytesSent ? totalSent - _lastBytesSent : 0;

            _lastBytesReceived = totalReceived;
            _lastBytesSent = totalSent;
            _lastPollTime = now;

            double downSpeed = diffReceived / seconds;
            double upSpeed = diffSent / seconds;

            CurrentMetrics = new NetworkMetrics
            {
                DownloadSpeedBytesPerSec = downSpeed,
                UploadSpeedBytesPerSec = upSpeed,
                DownloadSpeedFormatted = FormatSpeed(downSpeed),
                UploadSpeedFormatted = FormatSpeed(upSpeed),
                IsConnected = NetworkInterface.GetIsNetworkAvailable(),
                ActiveAdapterName = adapterName
            };

            NetworkUpdated?.Invoke(CurrentMetrics);
        }
        catch
        {
            // Tangani pengecualian jika adapter jaringan dinonaktifkan saat polling
        }
    }

    private static string FormatSpeed(double bytesPerSec)
    {
        if (bytesPerSec < 1024)
            return $"{bytesPerSec:F0} B/s";
        if (bytesPerSec < 1024 * 1024)
            return $"{(bytesPerSec / 1024.0):F1} KB/s";
        return $"{(bytesPerSec / (1024.0 * 1024.0)):F1} MB/s";
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
        _timer.Stop();
        _timer.Dispose();
    }
}
