using System;
using System.Timers;
using DynamicIsland.Models;

namespace DynamicIsland.Services;

public class TimerService : IDisposable
{
    private readonly System.Timers.Timer _pomodoroTimer;
    private readonly System.Timers.Timer _stopwatchTimer;
    private bool _isDisposed;

    // Status Pomodoro
    public PomodoroState PomodoroState { get; private set; } = PomodoroState.Stopped;
    public PomodoroSessionType CurrentSession { get; private set; } = PomodoroSessionType.Work;
    public TimeSpan PomodoroRemaining { get; private set; }
    public TimeSpan PomodoroTotalDuration { get; private set; }
    public double PomodoroProgress => PomodoroTotalDuration.TotalSeconds > 0
        ? ((PomodoroTotalDuration.TotalSeconds - PomodoroRemaining.TotalSeconds) / PomodoroTotalDuration.TotalSeconds) * 100.0
        : 0;

    // Status Stopwatch
    public bool IsStopwatchRunning { get; private set; }
    public TimeSpan StopwatchElapsed { get; private set; } = TimeSpan.Zero;

    // Event
    public event Action<TimeSpan, double>? PomodoroTicked;
    public event Action<PomodoroSessionType>? PomodoroFinished;
    public event Action<TimeSpan>? StopwatchTicked;

    public TimerService()
    {
        _pomodoroTimer = new System.Timers.Timer(1000);
        _pomodoroTimer.Elapsed += OnPomodoroElapsed;

        _stopwatchTimer = new System.Timers.Timer(1000);
        _stopwatchTimer.Elapsed += OnStopwatchElapsed;

        SetPomodoroSession(PomodoroSessionType.Work);
    }

    public void SetPomodoroSession(PomodoroSessionType sessionType)
    {
        CurrentSession = sessionType;
        PomodoroTotalDuration = sessionType switch
        {
            PomodoroSessionType.Work => TimeSpan.FromMinutes(25),
            PomodoroSessionType.ShortBreak => TimeSpan.FromMinutes(5),
            PomodoroSessionType.LongBreak => TimeSpan.FromMinutes(15),
            _ => TimeSpan.FromMinutes(25)
        };
        PomodoroRemaining = PomodoroTotalDuration;
        PomodoroState = PomodoroState.Stopped;
        _pomodoroTimer.Stop();

        PomodoroTicked?.Invoke(PomodoroRemaining, PomodoroProgress);
    }

    public void StartPomodoro()
    {
        if (PomodoroRemaining <= TimeSpan.Zero)
        {
            SetPomodoroSession(CurrentSession);
        }

        PomodoroState = PomodoroState.Running;
        _pomodoroTimer.Start();
        PomodoroTicked?.Invoke(PomodoroRemaining, PomodoroProgress);
    }

    public void PausePomodoro()
    {
        PomodoroState = PomodoroState.Paused;
        _pomodoroTimer.Stop();
        PomodoroTicked?.Invoke(PomodoroRemaining, PomodoroProgress);
    }

    public void ResetPomodoro()
    {
        SetPomodoroSession(CurrentSession);
    }

    private void OnPomodoroElapsed(object? sender, ElapsedEventArgs e)
    {
        if (PomodoroRemaining.TotalSeconds > 1)
        {
            PomodoroRemaining = PomodoroRemaining.Subtract(TimeSpan.FromSeconds(1));
            PomodoroTicked?.Invoke(PomodoroRemaining, PomodoroProgress);
        }
        else
        {
            PomodoroRemaining = TimeSpan.Zero;
            PomodoroState = PomodoroState.Stopped;
            _pomodoroTimer.Stop();

            PomodoroTicked?.Invoke(PomodoroRemaining, 100.0);
            PomodoroFinished?.Invoke(CurrentSession);

            // Beralih sesi otomatis (Work -> Short Break -> Work)
            if (CurrentSession == PomodoroSessionType.Work)
            {
                SetPomodoroSession(PomodoroSessionType.ShortBreak);
            }
            else
            {
                SetPomodoroSession(PomodoroSessionType.Work);
            }
        }
    }

    // =========================================================================
    // STOPWATCH
    // =========================================================================

    public void StartStopwatch()
    {
        IsStopwatchRunning = true;
        _stopwatchTimer.Start();
        StopwatchTicked?.Invoke(StopwatchElapsed);
    }

    public void PauseStopwatch()
    {
        IsStopwatchRunning = false;
        _stopwatchTimer.Stop();
        StopwatchTicked?.Invoke(StopwatchElapsed);
    }

    public void ResetStopwatch()
    {
        IsStopwatchRunning = false;
        _stopwatchTimer.Stop();
        StopwatchElapsed = TimeSpan.Zero;
        StopwatchTicked?.Invoke(StopwatchElapsed);
    }

    private void OnStopwatchElapsed(object? sender, ElapsedEventArgs e)
    {
        StopwatchElapsed = StopwatchElapsed.Add(TimeSpan.FromSeconds(1));
        StopwatchTicked?.Invoke(StopwatchElapsed);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _pomodoroTimer.Stop();
        _pomodoroTimer.Dispose();

        _stopwatchTimer.Stop();
        _stopwatchTimer.Dispose();
    }
}
