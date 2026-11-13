using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using DynamicIsland.Models;
using Windows.UI.Notifications.Management;
using Windows.UI.Notifications;

namespace DynamicIsland.Services;

/// <summary>
/// Layanan pendengar notifikasi Windows menggunakan UserNotificationListener.
/// Membutuhkan izin pengguna secara eksplisit saat pertama kali dijalankan.
/// </summary>
public class NotificationService : IDisposable
{
    private UserNotificationListener? _listener;
    private readonly DispatcherTimer _pollTimer;
    private readonly HashSet<uint> _dismissedIds = new();
    private bool _isListening  = false;
    private bool _isDisposed   = false;

    /// <summary>Koleksi notifikasi yang dapat di-binding ke UI (maks 30 item).</summary>
    public ObservableCollection<NotificationItem> Notifications { get; } = new();

    /// <summary>Apakah izin akses sudah diberikan pengguna.</summary>
    public bool IsAccessGranted { get; private set; } = false;

    /// <summary>Status teks izin (untuk ditampilkan di UI).</summary>
    public string AccessStatusText { get; private set; } = "Belum diminta";

    /// <summary>Dipicu saat ada notifikasi baru masuk.</summary>
    public event Action<NotificationItem>? NotificationReceived;

    /// <summary>Dipicu saat status izin berubah.</summary>
    public event Action<bool>? AccessStatusChanged;

    public NotificationService()
    {
        // DispatcherTimer berjalan di UI thread — aman untuk memodifikasi ObservableCollection
        _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _pollTimer.Tick += async (s, e) => await PollNotificationsAsync();
    }

    /// <summary>
    /// Meminta izin pengguna dan mulai memantau notifikasi.
    /// Harus dipanggil dari UI thread.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _listener = UserNotificationListener.Current;
            if (_listener == null)
            {
                IsAccessGranted = false;
                AccessStatusText = "Fitur notifikasi tidak didukung";
                AccessStatusChanged?.Invoke(false);
                return;
            }

            var accessStatus = await _listener.RequestAccessAsync();

            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                IsAccessGranted = accessStatus == UserNotificationListenerAccessStatus.Allowed;

                AccessStatusText = accessStatus switch
                {
                    UserNotificationListenerAccessStatus.Allowed     => "Aktif (Sinkron)",
                    UserNotificationListenerAccessStatus.Denied      => "Akses ditolak di Pengaturan Windows",
                    UserNotificationListenerAccessStatus.Unspecified => "Menunggu izin Windows",
                    _                                                => "Status tidak diketahui"
                };

                AccessStatusChanged?.Invoke(IsAccessGranted);
            });

            if (IsAccessGranted)
            {
                // Muat notifikasi yang sudah ada di notification center
                await LoadExistingNotificationsAsync();

                try
                {
                    // Daftarkan listener untuk notifikasi masuk real-time (bisa unsupported pada desktop win32 unpackaged)
                    _listener.NotificationChanged += OnNotificationChanged;
                    _isListening = true;
                }
                catch (Exception evEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[NotificationService] NotificationChanged event not supported: {evEx.Message}");
                }

                // Polling sebagai fallback periodik untuk sinkronisasi notifikasi
                _pollTimer.Start();
            }
        }
        catch (Exception ex)
        {
            // Catat log diagnosa error
            System.Diagnostics.Debug.WriteLine($"[NotificationService] Init error: {ex}");
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                IsAccessGranted = false;
                AccessStatusText = $"Gagal memuat notifikasi ({ex.Message})";
                AccessStatusChanged?.Invoke(false);
            });
        }
    }

    private async Task LoadExistingNotificationsAsync()
    {
        if (_listener == null) return;

        try
        {
            var notifications = await _listener.GetNotificationsAsync(NotificationKinds.Toast);
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                foreach (var notif in notifications)
                {
                    var item = ParseNotification(notif);
                    if (item != null && !Notifications.ContainsId(item.Id))
                    {
                        Notifications.Insert(0, item);
                    }
                }

                // Batasi 30 item
                TrimHistory();
            });
        }
        catch { /* Tangani error parsing secara senyap */ }
    }

    private async void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
    {
        if (args.ChangeKind == UserNotificationChangedKind.Added)
        {
            await PollNotificationsAsync();
        }
        else if (args.ChangeKind == UserNotificationChangedKind.Removed)
        {
            // Hapus dari koleksi lokal jika notifikasi dibatalkan dari Windows
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                for (int i = Notifications.Count - 1; i >= 0; i--)
                {
                    if (Notifications[i].Id == args.UserNotificationId)
                    {
                        Notifications.RemoveAt(i);
                        break;
                    }
                }
            });
        }
    }

    private async Task PollNotificationsAsync()
    {
        if (_listener == null) return;

        try
        {
            var notifications = await _listener.GetNotificationsAsync(NotificationKinds.Toast);

            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                foreach (var notif in notifications)
                {
                    // Abaikan notifikasi yang sudah pernah di-dismiss / dihapus oleh user
                    if (_dismissedIds.Contains(notif.Id))
                    {
                        continue;
                    }

                    var item = ParseNotification(notif);
                    if (item != null && !Notifications.ContainsId(item.Id))
                    {
                        Notifications.Insert(0, item);
                        NotificationReceived?.Invoke(item);
                        TrimHistory();
                    }
                }
            });
        }
        catch { /* Polling gagal — jangan crash */ }
    }

    /// <summary>Menguraikan UserNotification menjadi NotificationItem yang dapat di-binding.</summary>
    private static NotificationItem? ParseNotification(UserNotification notif)
    {
        try
        {
            var binding = notif.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            if (binding == null) return null;

            var textElements = binding.GetTextElements();
            string title = textElements.Count > 0 ? textElements[0].Text : string.Empty;
            string body  = textElements.Count > 1 ? textElements[1].Text : string.Empty;

            // Abaikan notifikasi kosong
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(body))
                return null;

            return new NotificationItem
            {
                Id         = notif.Id,
                AppName    = notif.AppInfo?.DisplayInfo?.DisplayName ?? "Sistem",
                Title      = title,
                Body       = body,
                ReceivedAt = notif.CreationTime.LocalDateTime
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Hapus semua notifikasi dari koleksi lokal (bukan dari Windows).</summary>
    public void ClearAllNotifications()
    {
        try
        {
            // Coba bersihkan dari Windows notification center
            _listener?.ClearNotifications();
        }
        catch { }

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            foreach (var item in Notifications)
            {
                _dismissedIds.Add(item.Id);
            }
            Notifications.Clear();
        });
    }

    /// <summary>Hapus satu notifikasi berdasarkan ID.</summary>
    public void DismissNotification(uint id)
    {
        // Tandai ID ini agar tidak dimasukkan kembali oleh polling berikutnya
        _dismissedIds.Add(id);

        try
        {
            // Hapus dari Windows notification center
            _listener?.RemoveNotification(id);
        }
        catch { /* Mungkin sudah hilang */ }

        // Hapus dari koleksi lokal
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            for (int i = Notifications.Count - 1; i >= 0; i--)
            {
                if (Notifications[i].Id == id)
                {
                    Notifications.RemoveAt(i);
                    break;
                }
            }
        });
    }

    private void TrimHistory()
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            while (Notifications.Count > 30)
            {
                Notifications.RemoveAt(Notifications.Count - 1);
            }
        });
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _pollTimer.Stop();

        if (_listener != null && _isListening)
        {
            _listener.NotificationChanged -= OnNotificationChanged;
        }
    }
}

/// <summary>Extension helper untuk mencari ID di dalam ObservableCollection.</summary>
internal static class NotificationCollectionExtensions
{
    public static bool ContainsId(this ObservableCollection<NotificationItem> collection, uint id)
    {
        foreach (var item in collection)
        {
            if (item.Id == id) return true;
        }
        return false;
    }
}
