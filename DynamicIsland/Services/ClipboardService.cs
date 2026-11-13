using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DynamicIsland.Models;

namespace DynamicIsland.Services;

public class ClipboardService : IDisposable
{
    private readonly DispatcherTimer _pollTimer;
    private string _lastClipboardText = string.Empty;
    private const int MaxHistoryLimit = 40;
    private bool _isDisposed;

    public ObservableCollection<ClipboardItem> History { get; } = new();
    public event Action? HistoryUpdated;

    public ClipboardService()
    {
        // Polling ringan 1.2 detik pada STA thread untuk menangkap perubahan teks
        _pollTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(1200)
        };
        _pollTimer.Tick += OnPollTick;
    }

    public void Start()
    {
        CheckClipboard();
        _pollTimer.Start();
    }

    public void Stop()
    {
        _pollTimer.Stop();
    }

    private void OnPollTick(object? sender, EventArgs e)
    {
        CheckClipboard();
    }

    private void CheckClipboard()
    {
        try
        {
            if (!Clipboard.ContainsText()) return;

            string currentText = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(currentText) || currentText == _lastClipboardText)
            {
                return;
            }

            _lastClipboardText = currentText;

            // Hapus entri lama jika teks yang sama sudah ada di riwayat
            var existing = History.FirstOrDefault(x => x.Text == currentText);
            if (existing != null)
            {
                History.Remove(existing);
            }

            // Tambahkan entri terbaru di paling atas
            History.Insert(0, new ClipboardItem
            {
                Text = currentText,
                Timestamp = DateTime.Now
            });

            // Batasi kapasitas riwayat
            while (History.Count > MaxHistoryLimit)
            {
                History.RemoveAt(History.Count - 1);
            }

            HistoryUpdated?.Invoke();
        }
        catch
        {
            // Abaikan kegagalan sementara saat aplikasi lain sedang mengunci clipboard
        }
    }

    public bool CopyToClipboard(string text)
    {
        try
        {
            _lastClipboardText = text;
            Clipboard.SetText(text);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void ClearHistory()
    {
        History.Clear();
        try
        {
            // Ambil teks sistem saat ini agar tidak langsung dimasukkan kembali oleh polling berikutnya
            if (Clipboard.ContainsText())
            {
                _lastClipboardText = Clipboard.GetText();
            }
            else
            {
                _lastClipboardText = string.Empty;
            }
        }
        catch
        {
            _lastClipboardText = string.Empty;
        }

        HistoryUpdated?.Invoke();
    }

    public IEnumerable<ClipboardItem> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return History;
        }

        return History.Where(x => x.Text.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _pollTimer.Stop();
    }
}
