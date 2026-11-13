using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace DynamicIsland.Services;

public class MediaTrackInfo
{
    public string Title { get; set; } = "Tidak Ada Media Aktif";
    public string Artist { get; set; } = "Buka pemutar musik (Spotify/Media Player)";
    public string AlbumTitle { get; set; } = string.Empty;
    public BitmapImage? Thumbnail { get; set; }
    public TimeSpan Position { get; set; } = TimeSpan.Zero;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    public double ProgressPercentage => Duration.TotalSeconds > 0 ? (Position.TotalSeconds / Duration.TotalSeconds) * 100.0 : 0;
    public bool IsPlaying { get; set; }
    public bool HasMedia { get; set; }
    public string SourceAppId { get; set; } = string.Empty;
    public string AppDisplayName { get; set; } = "MUSIK";
}

public class MediaService : IDisposable
{
    private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;
    private GlobalSystemMediaTransportControlsSession? _currentSession;
    private readonly System.Timers.Timer _timelineTimer;
    private bool _isDisposed;

    public event Action<MediaTrackInfo>? MediaChanged;
    public event Action<TimeSpan, TimeSpan, double>? TimelineUpdated;

    public MediaTrackInfo CurrentTrack { get; private set; } = new();

    public MediaService()
    {
        // Interval 500ms agar pembaruan progres bar terasa halus dan presisi
        _timelineTimer = new System.Timers.Timer(500);
        _timelineTimer.Elapsed += (s, e) => UpdateTimeline();
    }

    public async Task InitializeAsync()
    {
        try
        {
            _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            if (_sessionManager != null)
            {
                _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                UpdateCurrentSession(_sessionManager.GetCurrentSession());
            }
        }
        catch
        {
            CurrentTrack = new MediaTrackInfo { HasMedia = false };
            MediaChanged?.Invoke(CurrentTrack);
        }
    }

    private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
    {
        UpdateCurrentSession(sender.GetCurrentSession());
    }

    private void UpdateCurrentSession(GlobalSystemMediaTransportControlsSession? newSession)
    {
        if (_currentSession != null)
        {
            _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
            _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
            _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
        }

        _currentSession = newSession;

        if (_currentSession != null)
        {
            _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
            _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
            _currentSession.TimelinePropertiesChanged += OnTimelinePropertiesChanged;

            _ = RefreshMediaAsync();
            _timelineTimer.Start();
        }
        else
        {
            _timelineTimer.Stop();
            CurrentTrack = new MediaTrackInfo
            {
                Title = "Tidak Ada Media Aktif",
                Artist = "Mendukung Spotify, YouTube, & Pemutar Media",
                HasMedia = false,
                IsPlaying = false,
                AppDisplayName = "OFFLINE"
            };
            MediaChanged?.Invoke(CurrentTrack);
        }
    }

    private async void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
    {
        await RefreshMediaAsync();
    }

    private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
    {
        var playbackInfo = sender.GetPlaybackInfo();
        if (playbackInfo != null)
        {
            CurrentTrack.IsPlaying = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            MediaChanged?.Invoke(CurrentTrack);
            UpdateTimeline();
        }
    }

    private void OnTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
    {
        UpdateTimeline();
    }

    private void UpdateTimeline()
    {
        if (_currentSession == null) return;

        try
        {
            var timeline = _currentSession.GetTimelineProperties();
            if (timeline != null)
            {
                TimeSpan position = timeline.Position;

                // FUNGSI UTAMA: Interpolasi posisi waktu nyata saat musik sedang diputar
                if (CurrentTrack.IsPlaying && timeline.LastUpdatedTime != default)
                {
                    TimeSpan elapsedSinceUpdate = DateTimeOffset.UtcNow - timeline.LastUpdatedTime;
                    if (elapsedSinceUpdate > TimeSpan.Zero)
                    {
                        position = timeline.Position + elapsedSinceUpdate;
                    }
                }

                // Batasi posisi agar tidak melebihi durasi trek
                if (timeline.EndTime > TimeSpan.Zero && position > timeline.EndTime)
                {
                    position = timeline.EndTime;
                }

                CurrentTrack.Position = position;
                CurrentTrack.Duration = timeline.EndTime;
                TimelineUpdated?.Invoke(position, timeline.EndTime, CurrentTrack.ProgressPercentage);
            }
        }
        catch
        {
            // Tangani penutupan sesi secara mendadak
        }
    }

    public async Task RefreshMediaAsync()
    {
        if (_currentSession == null) return;

        try
        {
            var mediaProperties = await _currentSession.TryGetMediaPropertiesAsync();
            var playbackInfo = _currentSession.GetPlaybackInfo();
            var timeline = _currentSession.GetTimelineProperties();

            BitmapImage? thumbnailImage = null;
            if (mediaProperties?.Thumbnail != null)
            {
                thumbnailImage = await LoadThumbnailAsync(mediaProperties.Thumbnail);
            }

            string title = string.IsNullOrWhiteSpace(mediaProperties?.Title) ? "Trek Tidak Dikenal" : mediaProperties.Title;
            string artist = string.IsNullOrWhiteSpace(mediaProperties?.Artist) ? "Artis Tidak Dikenal" : mediaProperties.Artist;
            string appId = _currentSession.SourceAppUserModelId ?? string.Empty;
            bool isPlaying = playbackInfo?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

            TimeSpan pos = timeline?.Position ?? TimeSpan.Zero;
            if (isPlaying && timeline != null && timeline.LastUpdatedTime != default)
            {
                TimeSpan elapsed = DateTimeOffset.UtcNow - timeline.LastUpdatedTime;
                if (elapsed > TimeSpan.Zero)
                {
                    pos += elapsed;
                }
            }

            CurrentTrack = new MediaTrackInfo
            {
                Title = title,
                Artist = artist,
                AlbumTitle = mediaProperties?.AlbumTitle ?? string.Empty,
                Thumbnail = thumbnailImage,
                Position = pos,
                Duration = timeline?.EndTime ?? TimeSpan.Zero,
                IsPlaying = isPlaying,
                HasMedia = true,
                SourceAppId = appId,
                AppDisplayName = GetFriendlyAppName(appId)
            };

            MediaChanged?.Invoke(CurrentTrack);
        }
        catch
        {
            CurrentTrack = new MediaTrackInfo { HasMedia = false };
            MediaChanged?.Invoke(CurrentTrack);
        }
    }

    public static string GetFriendlyAppName(string? appId)
    {
        if (string.IsNullOrWhiteSpace(appId)) return "MUSIK";
        string lower = appId.ToLowerInvariant();
        if (lower.Contains("spotify")) return "SPOTIFY";
        if (lower.Contains("chrome")) return "CHROME";
        if (lower.Contains("edge")) return "EDGE";
        if (lower.Contains("firefox")) return "FIREFOX";
        if (lower.Contains("zunemusic") || lower.Contains("mediaplayer")) return "MEDIA PLAYER";
        if (lower.Contains("vlc")) return "VLC";
        if (lower.Contains("itunes") || lower.Contains("apple")) return "APPLE MUSIC";
        return "MUSIK";
    }

    private static async Task<BitmapImage?> LoadThumbnailAsync(IRandomAccessStreamReference streamRef)
    {
        try
        {
            using var stream = await streamRef.OpenReadAsync();
            using var netStream = stream.AsStreamForRead();
            var memoryStream = new MemoryStream();
            await netStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> TogglePlayPauseAsync()
    {
        if (_currentSession != null)
        {
            return await _currentSession.TryTogglePlayPauseAsync();
        }
        return false;
    }

    public async Task<bool> SkipNextAsync()
    {
        if (_currentSession != null)
        {
            return await _currentSession.TrySkipNextAsync();
        }
        return false;
    }

    public async Task<bool> SkipPreviousAsync()
    {
        if (_currentSession != null)
        {
            return await _currentSession.TrySkipPreviousAsync();
        }
        return false;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _timelineTimer.Stop();
        _timelineTimer.Dispose();

        if (_currentSession != null)
        {
            _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
            _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
            _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
            _currentSession = null;
        }

        if (_sessionManager != null)
        {
            _sessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
            _sessionManager = null;
        }
    }
}
