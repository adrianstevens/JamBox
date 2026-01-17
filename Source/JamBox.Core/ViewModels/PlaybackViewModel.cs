using Avalonia.Threading;
using JamBox.Core.Models;
using JamBox.Core.Services.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace JamBox.Core.ViewModels;

public class PlaybackViewModel : ViewModelBase
{
    private readonly IAudioPlayerService _audioPlayerService;
    private readonly IJellyfinApiService _jellyfinApiService;

    private ObservableCollection<Track> _playlist = [];
    public ObservableCollection<Track> Playlist
    {
        get => _playlist;
        private set => this.RaiseAndSetIfChanged(ref _playlist, value);
    }

    private Track? _selectedTrack;
    public Track? SelectedTrack
    {
        get => _selectedTrack;
        set => this.RaiseAndSetIfChanged(ref _selectedTrack, value);
    }

    private bool _isTrackPlaying = false;
    public bool IsTrackPlaying
    {
        get => _isTrackPlaying;
        set => this.RaiseAndSetIfChanged(ref _isTrackPlaying, value);
    }

    private bool _showNowPlaying;
    public bool ShowNowPlaying
    {
        get => _showNowPlaying;
        set => this.RaiseAndSetIfChanged(ref _showNowPlaying, value);
    }

    private PlaybackState _playback;
    public PlaybackState Playback
    {
        get => _playback;
        private set
        {
            this.RaiseAndSetIfChanged(ref _playback, value);
            IsTrackPlaying = value == PlaybackState.Playing;
            ShowNowPlaying = SelectedTrack != null;
        }
    }

    private string? _nowPlayingAlbumArtUrl = string.Empty;
    public string? NowPlayingAlbumArtUrl
    {
        get => _nowPlayingAlbumArtUrl;
        set => this.RaiseAndSetIfChanged(ref _nowPlayingAlbumArtUrl, value);
    }

    private string? _nowPlayingSongTitle = string.Empty;
    public string? NowPlayingSongTitle
    {
        get => _nowPlayingSongTitle;
        set => this.RaiseAndSetIfChanged(ref _nowPlayingSongTitle, value);
    }

    private string? _nowPlayingArtist = string.Empty;
    public string? NowPlayingArtist
    {
        get => _nowPlayingArtist;
        set => this.RaiseAndSetIfChanged(ref _nowPlayingArtist, value);
    }

    private string _nowPlayingElapsedTime = "0:00";
    public string NowPlayingElapsedTime
    {
        get => _nowPlayingElapsedTime;
        set => this.RaiseAndSetIfChanged(ref _nowPlayingElapsedTime, value);
    }

    private string _nowPlayingRemainingTime = "-0:00";
    public string NowPlayingRemainingTime
    {
        get => _nowPlayingRemainingTime;
        set => this.RaiseAndSetIfChanged(ref _nowPlayingRemainingTime, value);
    }

    private int _volume;
    public int Volume
    {
        get => _volume;
        set
        {
            if (_volume != value)
            {
                this.RaiseAndSetIfChanged(ref _volume, value);
                _audioPlayerService.Volume = value;
            }
        }
    }

    private double _seekPosition;
    public double SeekPosition
    {
        get => _seekPosition;
        set => this.RaiseAndSetIfChanged(ref _seekPosition, value);
    }

    private double _seekLength;
    public double SeekLength
    {
        get => _seekLength;
        set => this.RaiseAndSetIfChanged(ref _seekLength, value);
    }

    private bool _isUserSeeking;
    public bool IsUserSeeking
    {
        get => _isUserSeeking;
        set => this.RaiseAndSetIfChanged(ref _isUserSeeking, value);
    }

    public ReactiveCommand<Unit, Unit> PlayPauseCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayNextCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayPreviousCommand { get; }

    public PlaybackViewModel(
        IAudioPlayerService audioPlayerService,
        IJellyfinApiService jellyfinApiService)
    {
        _audioPlayerService = audioPlayerService;
        _jellyfinApiService = jellyfinApiService;

        _audioPlayerService.StateChanged += (_, state) =>
        {
            Playback = state;

            if (state == PlaybackState.EndReached)
            {
                PlayNextCommand!.Execute().Subscribe();
            }
        };

        _audioPlayerService.PositionChanged += (_, position) =>
        {
            if (!IsUserSeeking) { SeekPosition = position; }

            NowPlayingElapsedTime = FormatMs(position);
            var remaining = Math.Max(0, _audioPlayerService.LengthMs - position);
            NowPlayingRemainingTime = "-" + FormatMs(remaining);

            SeekLength = _audioPlayerService.LengthMs;
        };

        _audioPlayerService.VolumeChanged += (_, volume) => Volume = volume;
        Volume = _audioPlayerService.Volume;

        var canPlay = this.WhenAnyValue(vm => vm.SelectedTrack).Select(t => t != null);
        var canPause = this.WhenAnyValue(x => x.Playback).Select(s => s == PlaybackState.Playing);
        var canResume = this.WhenAnyValue(x => x.Playback).Select(s => s == PlaybackState.Paused);
        var canStop = this.WhenAnyValue(x => x.Playback).Select(s => s is PlaybackState.Playing or PlaybackState.Paused);
        var canToggle = this.WhenAnyValue(x => x.SelectedTrack, x => x.Playback, (track, state) => state == PlaybackState.Playing || track != null);
        var canPlayNext = this.WhenAnyValue(vm => vm.SelectedTrack, vm => vm.Playlist)
            .Select(t => t.Item1 != null && t.Item2.Count > 0 && t.Item2.IndexOf(t.Item1) + 1 < t.Item2.Count);
        var canPlayPrevious = this.WhenAnyValue(vm => vm.SelectedTrack, vm => vm.Playlist)
            .Select(t => t.Item1 != null && t.Item2.Count > 0 && t.Item2.IndexOf(t.Item1) > 0);

        PauseCommand = ReactiveCommand.Create(() => _audioPlayerService.Pause(), canPause);
        ResumeCommand = ReactiveCommand.Create(() => _audioPlayerService.Resume(), canResume);
        StopCommand = ReactiveCommand.Create(() => _audioPlayerService.Stop(), canStop);
        PlayCommand = ReactiveCommand.CreateFromTask(PlaySelectedTrackAsync, canPlay);
        PlayNextCommand = ReactiveCommand.CreateFromTask(PlayNextTrackAsync, canPlayNext);
        PlayPreviousCommand = ReactiveCommand.CreateFromTask(PlayPreviousTrackAsync, canPlayPrevious);
        PlayPauseCommand = ReactiveCommand.CreateFromTask(PlayPauseTrackAsync, canToggle);
    }

    /// <summary>
    /// Sets the playlist and current track for playback.
    /// Called by LibraryViewModel when user selects a track.
    /// </summary>
    public void SetPlaylist(ObservableCollection<Track> tracks, Track selectedTrack, string? albumArtUrl)
    {
        Playlist = tracks;
        SelectedTrack = selectedTrack;
        NowPlayingAlbumArtUrl = albumArtUrl;
    }

    private async Task PlaySelectedTrackAsync()
    {
        if (SelectedTrack == null) { return; }

        var headers = new Dictionary<string, string>
        {
            ["X-Emby-Token"] = _jellyfinApiService?.CurrentAccessToken ?? string.Empty
        };

        var baseUrl = _jellyfinApiService?.ServerUrl?.TrimEnd('/');
        var url = $"{baseUrl}/Items/{SelectedTrack.Id}/File?api_key={_jellyfinApiService?.CurrentAccessToken}";

        SelectedTrack.IsPlaying = true;
        NowPlayingSongTitle = SelectedTrack.Title;
        NowPlayingArtist = SelectedTrack.AlbumArtist;
        await _audioPlayerService.PlayAsync(url, headers);
    }

    private Task PlayPreviousTrackAsync()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            if (SelectedTrack is null || !Playlist.Any())
            {
                return;
            }

            var currentIndex = Playlist.IndexOf(SelectedTrack);
            var previousIndex = currentIndex - 1;
            if (previousIndex < 0)
            {
                return;
            }
            SelectedTrack = Playlist[previousIndex];
            await PlaySelectedTrackAsync();
        });

        return Task.CompletedTask;
    }

    private Task PlayNextTrackAsync()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            if (SelectedTrack is null || !Playlist.Any())
            {
                return;
            }

            var currentIndex = Playlist.IndexOf(SelectedTrack);
            var nextIndex = currentIndex + 1;
            if (nextIndex >= Playlist.Count)
            {
                return;
            }
            SelectedTrack = Playlist[nextIndex];
            await PlaySelectedTrackAsync();
        });

        return Task.CompletedTask;
    }

    private async Task PlayPauseTrackAsync()
    {
        if (Playback == PlaybackState.Stopped)
        {
            if (SelectedTrack is null) return;
            NowPlayingSongTitle = SelectedTrack.Title;
            await PlaySelectedTrackAsync();
        }
        else if (Playback == PlaybackState.Playing)
        {
            _audioPlayerService.Pause();
        }
        else if (Playback == PlaybackState.Paused)
        {
            _audioPlayerService.Resume();
        }
    }

    public void SeekTo(double positionMs)
    {
        _audioPlayerService.Seek((long)positionMs);
    }

    private static string FormatMs(long ms)
    {
        if (ms < 0) { ms = 0; }

        var trackTime = TimeSpan.FromMilliseconds(ms);

        return trackTime.Hours > 0
            ? $"{(int)trackTime.TotalHours}:{trackTime.Minutes:D2}:{trackTime.Seconds:D2}"
            : $"{trackTime.Minutes}:{trackTime.Seconds:D2}";
    }
}
