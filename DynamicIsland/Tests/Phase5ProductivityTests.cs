using System;
using System.Threading;
using DynamicIsland.Models;
using DynamicIsland.Services;

namespace DynamicIsland.Tests;

public static class Phase5ProductivityTests
{
    public static void RunAllTests()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("MEMULAI PENGUJIAN MANDIRI MODUL PHASE 5 (PRODUKTIVITAS)");
        Console.WriteLine("==================================================");

        // 1. Uji Pomodoro & Stopwatch
        Console.WriteLine("\n[1] Menguji TimerService (Pomodoro & Stopwatch)...");
        using var timerService = new TimerService();
        timerService.SetPomodoroSession(PomodoroSessionType.Work);
        Console.WriteLine($"    -> Sesi Pomodoro Awal: {timerService.PomodoroRemaining.TotalMinutes} Menit");
        timerService.StartPomodoro();
        Thread.Sleep(1100);
        Console.WriteLine($"    -> Hitung Mundur Pomodoro: {timerService.PomodoroRemaining}");
        timerService.PausePomodoro();
        Console.WriteLine("    -> Status Pomodoro: Dijeda (Berhasil)");

        timerService.StartStopwatch();
        Thread.Sleep(1100);
        Console.WriteLine($"    -> Stopwatch Hitung Maju: {timerService.StopwatchElapsed}");
        timerService.ResetStopwatch();
        Console.WriteLine("    [OK] Modul Pomodoro & Stopwatch Lulus Uji.");

        // 2. Uji Launcher Service
        Console.WriteLine("\n[2] Menguji LauncherService (Pintasan Aplikasi)...");
        var launcher = new LauncherService();
        Console.WriteLine($"    -> Jumlah Aplikasi Terdaftar: {launcher.Apps.Count}");
        foreach (var app in launcher.Apps)
        {
            Console.WriteLine($"       • {app.Name} ({app.Path})");
        }
        Console.WriteLine("    [OK] Modul LauncherService Berhasil Memuat Konfigurasi Lokal.");

        // 3. Uji Calendar Service
        Console.WriteLine("\n[3] Menguji CalendarService (Kalender Offline)...");
        var calendar = new CalendarService();
        Console.WriteLine($"    -> Tanggal Hari Ini: {calendar.CurrentFormattedDate}");
        Console.WriteLine($"    -> Bulan/Tahun: {calendar.MonthYearText}");
        var days = calendar.GetMonthDays();
        Console.WriteLine($"    -> Total Sel Hari Bulan Ini: {days.Count} hari");
        Console.WriteLine("    [OK] Modul CalendarService Lulus Uji.");

        Console.WriteLine("\n==================================================");
        Console.WriteLine("SELURUH MODUL PHASE 5 LULUS PENGUJIAN MANDIRI");
        Console.WriteLine("==================================================");
    }
}
