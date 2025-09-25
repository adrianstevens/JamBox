using JamBox.Core.Models;
using JamBox.Core.Services.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace JamBox.Core.ViewModels;

public class LibraryViewModel : ViewModelBase
{
    private readonly IAudioPlayer _audioPlayer;
    private readonly INavigationService _navigationService;
    private readonly IJellyfinApiService _jellyfinApiService;

    private MediaCollectionItem? _selectedLibrary;

    public ObservableCollection<Artist> Artists { get; private set; } = [];

    private Artist? _selectedArtist;
    public Artist? SelectedArtist
    {
        get => _selectedArtist;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedArtist, value);
            if (_selectedArtist != null)
            {
                AlbumSortStatus = "BY RELEASE YEAR";
                LoadAlbumsCommand.Execute().Subscribe();
                LoadTracksCommand.Execute().Subscribe();
            }
        }
    }

    private string? _artistCount;
    public string? ArtistCount
    {
        get => _artistCount;
        set => this.RaiseAndSetIfChanged(ref _artistCount, value);
    }

    private string _artistSortStatus = "A-Z";
    public string ArtistSortStatus
    {
        get => _artistSortStatus;
        set => this.RaiseAndSetIfChanged(ref _artistSortStatus, value);
    }

    public ObservableCollection<Album> Albums { get; } = [];

    private Album? _selectedAlbum;
    public Album? SelectedAlbum
    {
        get => _selectedAlbum;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedAlbum, value);
            if (_selectedAlbum != null)
            {
                TrackSortStatus = "BY ALBUM";
                LoadTracksCommand.Execute().Subscribe();
            }
        }
    }

    private string? _albumCount;
    public string? AlbumCount
    {
        get => _albumCount;
        set => this.RaiseAndSetIfChanged(ref _albumCount, value);
    }

    private string _albumSortStatus = "A-Z";
    public string AlbumSortStatus
    {
        get => _albumSortStatus;
        set => this.RaiseAndSetIfChanged(ref _albumSortStatus, value);
    }

    public ObservableCollection<Track> Tracks { get; } = [];

    private Track? _selectedTrack;
    public Track? SelectedTrack
    {
        get => _selectedTrack;
        set => this.RaiseAndSetIfChanged(ref _selectedTrack, value);
    }

    private string? _trackCount;
    public string? TrackCount
    {
        get => _trackCount;
        set => this.RaiseAndSetIfChanged(ref _trackCount, value);
    }

    private string _trackSortStatus = "A-Z";
    public string TrackSortStatus
    {
        get => _trackSortStatus;
        set => this.RaiseAndSetIfChanged(ref _trackSortStatus, value);
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

    private string? _nowPlayingAlbumArtUrl = "";
    public string? NowPlayingAlbumArtUrl
    {
        get => _nowPlayingAlbumArtUrl;
        set => this.RaiseAndSetIfChanged(ref _nowPlayingAlbumArtUrl, value);
    }

    private string? _nowPlayingSongTitle = "";
    public string? NowPlayingSongTitle
    {
        get => _nowPlayingSongTitle;
        set => this.RaiseAndSetIfChanged(ref _nowPlayingSongTitle, value);
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
                _audioPlayer.Volume = value;
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
    public ReactiveCommand<Unit, Unit> LoadArtistsCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadAlbumsCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadTracksCommand { get; }
    public ReactiveCommand<Unit, Unit> SortArtistsCommand { get; }
    public ReactiveCommand<Unit, Unit> SortAlbumsCommand { get; }
    public ReactiveCommand<Unit, Unit> SortTracksCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetArtistsSelectionCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetAlbumSelectionCommand { get; }
    public ReactiveCommand<Unit, Unit> JukeBoxModeCommand { get; }

    public LibraryViewModel(
        IAudioPlayer audioPlayer,
        INavigationService navigationService,
        IJellyfinApiService jellyfinApiService)
    {
        _audioPlayer = audioPlayer;
        _navigationService = navigationService;
        _jellyfinApiService = jellyfinApiService;

        _audioPlayer.StateChanged += (_, state) => Playback = state;

        _audioPlayer.PositionChanged += (_, position) =>
        {
            if (!IsUserSeeking) { SeekPosition = position; }

            NowPlayingElapsedTime = FormatMs(position);
            var remaining = Math.Max(0, _audioPlayer.LengthMs - position);
            NowPlayingRemainingTime = "-" + FormatMs(remaining);

            SeekLength = _audioPlayer.LengthMs;
        };

        _audioPlayer.VolumeChanged += (_, volume) => Volume = volume;
        Volume = _audioPlayer.Volume;

        LoadArtistsCommand = ReactiveCommand.CreateFromTask(LoadArtistsAsync);
        LoadAlbumsCommand = ReactiveCommand.CreateFromTask(LoadAlbumsAsync);
        LoadTracksCommand = ReactiveCommand.CreateFromTask(LoadTracksAsync);
        SortArtistsCommand = ReactiveCommand.CreateFromTask(SortArtistsAsync);
        SortAlbumsCommand = ReactiveCommand.CreateFromTask(SortAlbumsAsync);
        SortTracksCommand = ReactiveCommand.CreateFromTask(SortTracksAsync);

        ResetArtistsSelectionCommand = ReactiveCommand.CreateFromTask(ResetArtistsSelectionAsync);
        ResetAlbumSelectionCommand = ReactiveCommand.CreateFromTask(ResetAlbumSelectionAsync);

        var canPlay = this.WhenAnyValue(vm => vm.SelectedTrack).Select(t => t != null);
        var canPause = this.WhenAnyValue(x => x.Playback).Select(s => s == PlaybackState.Playing);
        var canResume = this.WhenAnyValue(x => x.Playback).Select(s => s == PlaybackState.Paused);
        var canStop = this.WhenAnyValue(x => x.Playback).Select(s => s is PlaybackState.Playing or PlaybackState.Paused);
        var canToggle = this.WhenAnyValue(x => x.SelectedTrack, x => x.Playback, (track, state) => state == PlaybackState.Playing || track != null);
        var canPlayNext = this.WhenAnyValue(vm => vm.SelectedTrack, vm => vm.Tracks)
            .Select(t => t.Item1 != null && t.Item2.Count > 0 && t.Item2.IndexOf(t.Item1) + 1 < t.Item2.Count);
        var canPlayPrevious = this.WhenAnyValue(vm => vm.SelectedTrack, vm => vm.Tracks)
            .Select(t => t.Item1 != null && t.Item2.Count > 0 && t.Item2.IndexOf(t.Item1) > 0);

        PauseCommand = ReactiveCommand.Create(() => _audioPlayer.Pause(), canPause);
        ResumeCommand = ReactiveCommand.Create(() => _audioPlayer.Resume(), canResume);
        StopCommand = ReactiveCommand.Create(() => _audioPlayer.Stop(), canStop);
        PlayCommand = ReactiveCommand.CreateFromTask(PlaySelectedTrackAsync, canPlay);
        PlayNextCommand = ReactiveCommand.CreateFromTask(PlayNextTrackAsync, canPlayNext);
        PlayPreviousCommand = ReactiveCommand.CreateFromTask(PlayPreviousTrackAsync, canPlayPrevious);
        PlayPauseCommand = ReactiveCommand.CreateFromTask(PlayPauseTrackAsync, canToggle);
        JukeBoxModeCommand = ReactiveCommand.Create(() => _navigationService.NavigateTo<JukeBoxPage, JukeBoxViewModel>());

        _ = LoadLibraryAsync();
    }

    private void SetArtists(IEnumerable<Artist> artists)
    {
        Artists = new ObservableCollection<Artist>(artists);
        this.RaisePropertyChanged(nameof(Artists));
        ArtistCount = $"{Artists.Count} ARTISTS";
    }

    private async Task LoadLibraryAsync()
    {
        var libraries = await _jellyfinApiService.GetUserMediaViewsAsync();
        _selectedLibrary = libraries.FirstOrDefault(lib => lib.CollectionType == "music");

        if (_selectedLibrary != null)
        {
            await Task.WhenAll(LoadArtistsAsync(), LoadAlbumsAsync());
        }
    }

    private async Task LoadArtistsAsync()
    {
        if (_selectedLibrary is null) { return; }

        Artists.Clear();

        var artists = await _jellyfinApiService.GetArtistsAsync(_selectedLibrary.Id);

        if (ArtistSortStatus == "A-Z")
        {
            artists = artists.OrderBy(a => a.Name).ToList();
        }
        else if (ArtistSortStatus == "Z-A")
        {
            artists = artists.OrderByDescending(a => a.Name).ToList();
        }

        SetArtists(artists);

        ArtistCount = $"{Artists.Count} ARTISTS";
    }

    private async Task LoadAlbumsAsync()
    {
        if (_selectedLibrary is null) { return; }

        List<Album>? albums = [];

        Albums.Clear();

        if (SelectedArtist == null)
        {
            albums = await _jellyfinApiService.GetAlbumsAsync(_selectedLibrary.Id);
        }
        else
        {
            albums = await _jellyfinApiService.GetAlbumsByArtistAsync(SelectedArtist.Id);
        }

        if (AlbumSortStatus == "A-Z")
        {
            albums = albums.OrderBy(a => a.Title).ToList();
        }
        else if (AlbumSortStatus == "BY RELEASE YEAR")
        {
            albums = albums.OrderByDescending(a => a.ProductionYear).ToList();
        }
        else if (AlbumSortStatus == "BY RATING")
        {
            albums = albums.OrderByDescending(a => a.UserData.IsFavorite).ToList();
        }

        foreach (var album in albums)
        {
            album.AlbumArtUrl = album.GetPrimaryImageUrl(_jellyfinApiService.ServerUrl ?? "", _jellyfinApiService.CurrentAccessToken ?? "") ?? "";

            album.AlbumSubtitle = SelectedArtist == null ? album.AlbumArtist : album.ProductionYear.ToString();
            Albums.Add(album);
        }

        AlbumCount = $"{Albums.Count} ALBUMS";
    }

    private async Task LoadTracksAsync()
    {
        if (_selectedLibrary is null) { return; }

        List<Track>? tracks = [];

        Tracks.Clear();

        if (SelectedArtist is not null && SelectedAlbum is null)
        {
            tracks = await _jellyfinApiService.GetTracksByArtistAsync(SelectedArtist.Id);
        }
        else if (SelectedAlbum is not null)
        {
            tracks = await _jellyfinApiService.GetTracksByAlbumAsync(SelectedAlbum.Id);
        }
        else
        {
            tracks = await _jellyfinApiService.GetTracksAsync(_selectedLibrary.Id);
        }

        if (TrackSortStatus == "A-Z")
        {
            tracks = tracks.OrderBy(t => t.Title).ToList();
        }
        else if (TrackSortStatus == "BY ALBUM")
        {
            tracks = tracks.OrderBy(t => t.IndexNumber).ToList();
        }
        //else if (TrackSortStatus == "BY RATING")
        //{
        //    tracks = tracks.OrderByDescending(t => t.CommunityRating).ToList();
        //}

        foreach (var track in tracks)
        {
            Tracks.Add(track);
        }

        TrackCount = $"{Tracks.Count} TRACKS";
    }

    private async Task SortArtistsAsync()
    {
        ArtistSortStatus = ArtistSortStatus == "A-Z" ? "Z-A" : "A-Z";
        await LoadArtistsAsync();
    }

    private async Task SortAlbumsAsync()
    {
        AlbumSortStatus = AlbumSortStatus == "A-Z" ? "BY RELEASE YEAR" : AlbumSortStatus == "BY RELEASE YEAR" ? "BY RATING" : "A-Z";
        await LoadAlbumsAsync();
    }

    private async Task SortTracksAsync()
    {
        if (SelectedAlbum == null)
        {
            TrackSortStatus = TrackSortStatus == "A-Z" ? "BY RATING" : "A-Z";
        }
        else
        {
            TrackSortStatus = TrackSortStatus == "A-Z" ? "BY ALBUM" : TrackSortStatus == "BY ALBUM" ? "BY RATING" : "A-Z";
        }


        await LoadTracksAsync();
    }

    private async Task ResetArtistsSelectionAsync()
    {
        SelectedArtist = null;
        await LoadArtistsAsync();
        AlbumSortStatus = "A-Z";
        await LoadAlbumsAsync();
        TrackSortStatus = "A-Z";
        await LoadTracksAsync();
    }

    private async Task ResetAlbumSelectionAsync()
    {
        SelectedAlbum = null;
        AlbumSortStatus = "A-Z";
        await LoadAlbumsAsync();
        TrackSortStatus = "A-Z";
        await LoadTracksAsync();
    }

    private async Task PlaySelectedTrackAsync()
    {
        if (SelectedTrack == null) { return; }

        var headers = new Dictionary<string, string>
        {
            ["X-Emby-Token"] = _jellyfinApiService?.CurrentAccessToken ?? string.Empty
        };

        //debug url to test playback without auth
        // url = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3";

        var baseUrl = _jellyfinApiService?.ServerUrl?.TrimEnd('/');

        // The original file endpoint (no transcoding):
        var url = $"{baseUrl}/Items/{SelectedTrack.Id}/File?api_key={_jellyfinApiService?.CurrentAccessToken}";

        if (SelectedAlbum == null)
        {
            var album = Albums.FirstOrDefault(a => a.Id == SelectedTrack.AlbumId);
            NowPlayingAlbumArtUrl = album?.AlbumArtUrl;
        }
        else
        {
            NowPlayingAlbumArtUrl = SelectedAlbum?.AlbumArtUrl;
        }

        NowPlayingSongTitle = SelectedTrack.Title;
        await _audioPlayer.PlayAsync(url, headers);
    }

    private async Task PlayPreviousTrackAsync()
    {
        if (SelectedTrack is null || !Tracks.Any())
        {
            return;
        }

        var currentIndex = Tracks.IndexOf(SelectedTrack);
        var previousIndex = currentIndex - 1;
        SelectedTrack = Tracks[previousIndex];
        await PlaySelectedTrackAsync();
    }

    private async Task PlayNextTrackAsync()
    {
        if (SelectedTrack is null || !Tracks.Any())
        {
            return;
        }

        var currentIndex = Tracks.IndexOf(SelectedTrack);
        var nextIndex = currentIndex + 1;
        SelectedTrack = Tracks[nextIndex];
        await PlaySelectedTrackAsync();
    }

    private async Task PlayPauseTrackAsync()
    {
        if (Playback == PlaybackState.Stopped)
        {
            if (SelectedTrack is null) return;
            NowPlayingAlbumArtUrl = SelectedAlbum?.AlbumArtUrl ?? null;
            NowPlayingSongTitle = SelectedTrack.Title;
            await PlaySelectedTrackAsync();
        }
        else if (Playback == PlaybackState.Playing)
        {
            _audioPlayer.Pause();
        }
        else if (Playback == PlaybackState.Paused)
        {
            _audioPlayer.Resume();
        }
    }

    public void SeekTo(double positionMs)
    {
        _audioPlayer.Seek((long)positionMs);
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