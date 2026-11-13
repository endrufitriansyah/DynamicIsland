using System;

namespace DynamicIsland.Models;

public enum PomodoroState
{
    Stopped,
    Running,
    Paused
}

public enum PomodoroSessionType
{
    Work,       // 25 menit
    ShortBreak, // 5 menit
    LongBreak   // 15 menit
}

public class ClipboardItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string DisplayTime => Timestamp.ToString("HH:mm");
    public string PreviewText => Text.Length > 80 ? Text[..77] + "…" : Text;
}

public class LauncherAppItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    /// <summary>Direktori kerja saat proses diluncurkan. Kosong = gunakan folder home pengguna.</summary>
    public string WorkingDirectory { get; set; } = string.Empty;
    public Controls.LucideIconKind IconKind { get; set; } = Controls.LucideIconKind.Launcher;
}
