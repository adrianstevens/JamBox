using JamBox.Core.Services.Interfaces;
using LibVLCSharp.Shared;

namespace JamBox.Core.Audio;

public sealed class AudioPlayer : IAudioPlayer
{
    private readonly LibVLC _libvlc;
    private readonly MediaPlayer _player;

    private Media? _currentMedia;
    private PlaybackState _state = PlaybackState.Stopped;

    public int Volume
    {
        get => _player.Volume; // LibVLC uses 0..100
        set
        {
            var v = Math.Clamp(value, 0, 100);
            if (_player.Volume != v)
            {
                _player.Volume = v;
                VolumeChanged?.Invoke(this, v);
            }
        }
    }

    public PlaybackState State => _state;

    public long PositionMs => _player.Time;

    public long LengthMs => _player.Length;

    public event EventHandler<int>? VolumeChanged;

    public event EventHandler<long>? PositionChanged;

    public event EventHandler<PlaybackState>? StateChanged;

    public AudioPlayer()
    {
        global::LibVLCSharp.Shared.Core.Initialize();

        _libvlc = new LibVLC(
            "--no-video",
            "--quiet",
            "--intf", "dummy",
            "--no-osd",
            "--network-caching=1000"
        );

        _player = new MediaPlayer(_libvlc);

        _player.TimeChanged += (_, e) => PositionChanged?.Invoke(this, e.Time);
        _player.Playing += (_, __) => UpdateState(PlaybackState.Playing);
        _player.Paused += (_, __) => UpdateState(PlaybackState.Paused);
        _player.Stopped += (_, __) => UpdateState(PlaybackState.Stopped);
        _player.EndReached += (_, __) => UpdateState(PlaybackState.Stopped);

        _player.EncounteredError += (_, __) => System.Diagnostics.Debug.WriteLine("[VLC] EncounteredError");

        if (_player.Volume == -1) { _player.Volume = 80; }
    }

    public Task PlayAsync(string url, IDictionary<string, string>? headers = null)
    {
        Stop();

        _currentMedia?.Dispose();
        _currentMedia = new Media(_libvlc, new Uri(url));

        if (headers is not null)
        {
            foreach (var kv in headers.Where(kv => !string.IsNullOrWhiteSpace(kv.Key)))
            {
                _currentMedia.AddOption($":http-header={kv.Key}: {kv.Value}");
            }
        }

        _player.Media = _currentMedia;

        var ok = _player.Play();

        if (!ok)
        {
            throw new InvalidOperationException("LibVLC failed to start playback.");
        }

        return Task.CompletedTask;
    }

    public void Pause() => _player.Pause();

    public void Resume() => _player.SetPause(false);

    public void Stop()
    {
        if (_player.IsPlaying) _player.Stop();
        UpdateState(PlaybackState.Stopped);
    }

    public void Seek(long positionMs)
    {
        if (positionMs < 0) positionMs = 0;
        _player.Time = positionMs;
    }

    private void UpdateState(PlaybackState s)
    {
        if (_state == s) return;
        _state = s;
        StateChanged?.Invoke(this, s);
    }

    public void Dispose()
    {
        try
        {
            _player?.Dispose();
            _currentMedia?.Dispose();
            _libvlc?.Dispose();
        }
        catch { /* swallow dispose */ }
    }
}