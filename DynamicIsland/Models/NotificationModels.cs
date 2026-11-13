using System;

namespace DynamicIsland.Models;

/// <summary>Model data untuk satu entri notifikasi yang diterima dari Windows.</summary>
public class NotificationItem
{
    public uint    Id          { get; set; }
    public string  AppName     { get; set; } = string.Empty;
    public string  Title       { get; set; } = string.Empty;
    public string  Body        { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.Now;

    /// <summary>Waktu ditampilkan dalam format HH:mm.</summary>
    public string DisplayTime => ReceivedAt.ToString("HH:mm");

    /// <summary>Pratinjau body maksimal 90 karakter.</summary>
    public string BodyPreview => Body.Length > 90 ? Body[..87] + "…" : Body;

    /// <summary>Label nama aplikasi pengirim (maks 18 karakter).</summary>
    public string AppLabel => AppName.Length > 18 ? AppName[..16] + "…" : AppName;
}
