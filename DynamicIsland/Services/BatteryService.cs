using System;
using Windows.System.Power;

namespace DynamicIsland.Services;

public class BatteryInfo
{
    public int Percentage { get; set; } = 100;
    public bool HasBattery { get; set; }
    public bool IsCharging { get; set; }
    public TimeSpan RemainingTime { get; set; } = TimeSpan.Zero;
    public string StatusText { get; set; } = "Daya Listrik AC";
}

public class BatteryService : IDisposable
{
    private bool _isDisposed;
    public event Action<BatteryInfo>? BatteryChanged;
    public BatteryInfo CurrentInfo { get; private set; } = new();

    public void Initialize()
    {
        try
        {
            PowerManager.BatteryStatusChanged += OnBatteryStatusChanged;
            PowerManager.RemainingChargePercentChanged += OnRemainingChargePercentChanged;
            PowerManager.RemainingDischargeTimeChanged += OnRemainingDischargeTimeChanged;

            UpdateBatteryInfo();
        }
        catch
        {
            CurrentInfo = new BatteryInfo
            {
                HasBattery = false,
                Percentage = 100,
                IsCharging = true,
                StatusText = "Daya Listrik AC / Desktop"
            };
            BatteryChanged?.Invoke(CurrentInfo);
        }
    }

    private void OnBatteryStatusChanged(object? sender, object e) => UpdateBatteryInfo();
    private void OnRemainingChargePercentChanged(object? sender, object e) => UpdateBatteryInfo();
    private void OnRemainingDischargeTimeChanged(object? sender, object e) => UpdateBatteryInfo();

    public void UpdateBatteryInfo()
    {
        try
        {
            var status = PowerManager.BatteryStatus;
            bool hasBattery = status != BatteryStatus.NotPresent;

            if (!hasBattery)
            {
                CurrentInfo = new BatteryInfo
                {
                    HasBattery = false,
                    Percentage = 100,
                    IsCharging = true,
                    StatusText = "Daya AC (Desktop)"
                };
            }
            else
            {
                int percent = PowerManager.RemainingChargePercent;
                bool isCharging = status == BatteryStatus.Charging;
                TimeSpan dischargeTime = PowerManager.RemainingDischargeTime;

                string statusText = isCharging ? "Mengisi Daya" : $"{percent}% Tersisa";

                CurrentInfo = new BatteryInfo
                {
                    HasBattery = true,
                    Percentage = percent,
                    IsCharging = isCharging,
                    RemainingTime = dischargeTime,
                    StatusText = statusText
                };
            }

            BatteryChanged?.Invoke(CurrentInfo);
        }
        catch
        {
            CurrentInfo = new BatteryInfo { HasBattery = false, Percentage = 100, StatusText = "Daya AC" };
            BatteryChanged?.Invoke(CurrentInfo);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        try
        {
            PowerManager.BatteryStatusChanged -= OnBatteryStatusChanged;
            PowerManager.RemainingChargePercentChanged -= OnRemainingChargePercentChanged;
            PowerManager.RemainingDischargeTimeChanged -= OnRemainingDischargeTimeChanged;
        }
        catch
        {
        }
    }
}
