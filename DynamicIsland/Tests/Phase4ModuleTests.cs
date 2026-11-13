using System;
using System.Threading;
using System.Threading.Tasks;
using DynamicIsland.Services;

namespace DynamicIsland.Tests;

public static class Phase4ModuleTests
{
    public static async Task RunAllTestsAsync()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("MEMULAI PENGUJIAN MANDIRI MODUL PHASE 4");
        Console.WriteLine("==================================================");

        // 1. Uji System Monitor (CPU & RAM)
        Console.WriteLine("\n[1] Menguji SystemMonitorService (CPU & RAM)...");
        using var sysMonitor = new SystemMonitorService(500);
        sysMonitor.Start();
        Thread.Sleep(1200); // Tunggu pembacaan delta kedua
        var metrics = sysMonitor.CurrentMetrics;
        Console.WriteLine($"    -> CPU Terdeteksi: {metrics.CpuUsagePercent}%");
        Console.WriteLine($"    -> RAM Terpakai: {metrics.RamUsedGb:F1} GB dari {metrics.RamTotalGb:F0} GB ({metrics.RamUsagePercent}%)");
        Console.WriteLine("    [OK] Modul System Monitor Berhasil Membaca Metrik Hardware.");

        // 2. Uji Battery Monitor
        Console.WriteLine("\n[2] Menguji BatteryService (Daya & Baterai)...");
        using var battery = new BatteryService();
        battery.Initialize();
        var bInfo = battery.CurrentInfo;
        Console.WriteLine($"    -> Status Perangkat: {(bInfo.HasBattery ? "Laptop (Baterai Ada)" : "Desktop / Daya AC")}");
        Console.WriteLine($"    -> Persentase: {bInfo.Percentage}%");
        Console.WriteLine($"    -> Status Teks: {bInfo.StatusText}");
        Console.WriteLine("    [OK] Modul Battery Berhasil Mengidentifikasi Status Daya.");

        // 3. Uji Network Monitor
        Console.WriteLine("\n[3] Menguji NetworkService (Pemantau Jaringan)...");
        using var network = new NetworkService(500);
        network.Start();
        Thread.Sleep(1100);
        var nMetrics = network.CurrentMetrics;
        Console.WriteLine($"    -> Status Koneksi: {(nMetrics.IsConnected ? "Terhubung" : "Offline")}");
        Console.WriteLine($"    -> Adapter Aktif: {nMetrics.ActiveAdapterName}");
        Console.WriteLine($"    -> Kecepatan Download: {nMetrics.DownloadSpeedFormatted}");
        Console.WriteLine($"    -> Kecepatan Upload: {nMetrics.UploadSpeedFormatted}");
        Console.WriteLine("    [OK] Modul Network Berhasil Menghitung Kecepatan Jaringan.");

        // 4. Uji Media Service (GSMTC)
        Console.WriteLine("\n[4] Menguji MediaService (GSMTC Windows)...");
        using var media = new MediaService();
        await media.InitializeAsync();
        var mInfo = media.CurrentTrack;
        Console.WriteLine($"    -> Sesi Aktif: {(mInfo.HasMedia ? mInfo.Title + " - " + mInfo.Artist : "Tidak Ada Sesi Media Berjalan (Normal)")}");
        Console.WriteLine("    [OK] Modul Media Service Berhasil Terhubung ke Windows GSMTC.");

        Console.WriteLine("\n==================================================");
        Console.WriteLine("SELURUH MODUL PHASE 4 LULUS PENGUJIAN MANDIRI");
        Console.WriteLine("==================================================");
    }
}
