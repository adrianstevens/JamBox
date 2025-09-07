using JamBox.Core.JellyFin;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace JamBox.Core.ViewModels;

public class LibraryViewModel : ViewModelBase
{
    private readonly JellyfinApiService _jellyfinService;
    private BaseItemDto _selectedLibrary;

    public ObservableCollection<Artist> Artists { get; } = [];

    private Artist _selectedArtist;
    public Artist SelectedArtist
    {
        get => _selectedArtist;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedArtist, value);
            if (_selectedArtist != null)
            {
                LoadAlbumsCommand.Execute().Subscribe();
            }
        }
    }

    private string _artistCount;
    public string ArtistCount
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

    private Album _selectedAlbum;
    public Album SelectedAlbum
    {
        get => _selectedAlbum;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedAlbum, value);
            if (_selectedAlbum != null)
            {
                LoadTracksCommand.Execute().Subscribe();
            }
        }
    }

    private string _albumCount;
    public string AlbumCount
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
    private Track _selectedTrack;
    public Track SelectedTrack
    {
        get => _selectedTrack;
        set => this.RaiseAndSetIfChanged(ref _selectedTrack, value);
    }

    private string _trackCount;
    public string TrackCount
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

    public ReactiveCommand<Unit, Unit> LoadArtistsCommand { get; }

    public ReactiveCommand<Unit, Unit> LoadAlbumsCommand { get; }

    public ReactiveCommand<Unit, Unit> LoadTracksCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleArtistSortCommand { get; }

    public ReactiveCommand<Unit, Unit> PlaySelectedTrackCommand { get; }

    public LibraryViewModel(JellyfinApiService jellyfinService)
    {
        _jellyfinService = jellyfinService;

        LoadArtistsCommand = ReactiveCommand.CreateFromTask(LoadArtistsAsync);
        LoadAlbumsCommand = ReactiveCommand.CreateFromTask(LoadAlbumsAsync);
        LoadTracksCommand = ReactiveCommand.CreateFromTask(LoadTracksAsync);
        ToggleArtistSortCommand = ReactiveCommand.CreateFromTask(SortArtistsAsync);

        var canPlay = this.WhenAnyValue(vm => vm.SelectedTrack).Select(t => t != null);
        PlaySelectedTrackCommand = ReactiveCommand.CreateFromTask(PlaySelectedTrackAsync, canPlay);

        //LoadArtistsCommand.Execute().Subscribe();

        _ = LoadLibraryAsync();
    }

    private async Task LoadLibraryAsync()
    {
        var libraries = await _jellyfinService.GetUserMediaViewsAsync();
        _selectedLibrary = libraries.FirstOrDefault(lib => lib.CollectionType == "music");

        if (_selectedLibrary != null)
        {
            await LoadArtistsAsync();
        }
    }

    private async Task LoadArtistsAsync()
    {
        Artists.Clear();
        var artists = await _jellyfinService.GetArtistsAsync(_selectedLibrary.Id);

        if (ArtistSortStatus == "A-Z")
        {
            artists = artists.OrderBy(a => a.Name).ToList();
        }
        else if (ArtistSortStatus == "Z-A")
        {
            artists = artists.OrderByDescending(a => a.Name).ToList();
        }

        foreach (var artist in artists)
        {
            Artists.Add(artist);
        }

        ArtistCount = $"{Artists.Count} ARTISTS";
    }

    private async Task LoadAlbumsAsync()
    {
        if (SelectedArtist == null)
        {
            return;
        }

        Albums.Clear();
        var albums = await _jellyfinService.GetAlbumsByArtistAsync(SelectedArtist.Id);
        foreach (var album in albums)
        {
            album.AlbumArtUrl = album.GetPrimaryImageUrl(_jellyfinService.ServerUrl, _jellyfinService.CurrentAccessToken, 140, 140);
            Albums.Add(album);
        }

        AlbumCount = $"{Albums.Count} ALBUMS";
    }

    private async Task LoadTracksAsync()
    {
        if (SelectedAlbum == null) { return; }

        Tracks.Clear();
        var tracks = await _jellyfinService.GetTracksByAlbumAsync(SelectedAlbum.Id);
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

    private async Task PlaySelectedTrackAsync()
    {
        if (SelectedTrack == null) { return; }

        var sessions = await _jellyfinService.GetSessionsAsync();
        if (sessions.Count > 0)
        {
            var session = sessions[0];
            await _jellyfinService.PlayTrackAsync(session.Id, SelectedTrack.Id);
        }
    }
}