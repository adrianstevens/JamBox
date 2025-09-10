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
                AlbumSortStatus = "BY RELEASE YEAR";
                LoadAlbumsCommand.Execute().Subscribe();
                LoadTracksCommand.Execute().Subscribe();
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
                TrackSortStatus = "BY ALBUM";
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

    public ReactiveCommand<Unit, Unit> SortArtistsCommand { get; }

    public ReactiveCommand<Unit, Unit> SortAlbumsCommand { get; }

    public ReactiveCommand<Unit, Unit> SortTracksCommand { get; }

    public ReactiveCommand<Unit, Unit> ResetArtistsSelectionCommand { get; }

    public ReactiveCommand<Unit, Unit> ResetAlbumSelectionCommand { get; }

    public ReactiveCommand<Unit, Unit> PlaySelectedTrackCommand { get; }

    public LibraryViewModel(JellyfinApiService jellyfinService)
    {
        _jellyfinService = jellyfinService;

        LoadArtistsCommand = ReactiveCommand.CreateFromTask(LoadArtistsAsync);
        LoadAlbumsCommand = ReactiveCommand.CreateFromTask(LoadAlbumsAsync);
        LoadTracksCommand = ReactiveCommand.CreateFromTask(LoadTracksAsync);
        SortArtistsCommand = ReactiveCommand.CreateFromTask(SortArtistsAsync);
        SortAlbumsCommand = ReactiveCommand.CreateFromTask(SortAlbumsAsync);
        SortTracksCommand = ReactiveCommand.CreateFromTask(SortTracksAsync);

        ResetArtistsSelectionCommand = ReactiveCommand.CreateFromTask(ResetArtistsSelectionAsync);
        ResetAlbumSelectionCommand = ReactiveCommand.CreateFromTask(ResetAlbumSelectionAsync);

        var canPlay = this.WhenAnyValue(vm => vm.SelectedTrack).Select(t => t != null);
        PlaySelectedTrackCommand = ReactiveCommand.CreateFromTask(PlaySelectedTrackAsync, canPlay);

        _ = LoadLibraryAsync();
    }

    private async Task LoadLibraryAsync()
    {
        var libraries = await _jellyfinService.GetUserMediaViewsAsync();
        _selectedLibrary = libraries.FirstOrDefault(lib => lib.CollectionType == "music");

        if (_selectedLibrary != null)
        {
            await LoadArtistsAsync();
            await LoadAlbumsAsync();
            await LoadTracksAsync();
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
        List<Album>? albums = [];

        Albums.Clear();

        if (SelectedArtist == null)
        {
            albums = await _jellyfinService.GetAlbumsAsync(_selectedLibrary.Id);
        }
        else
        {
            albums = await _jellyfinService.GetAlbumsByArtistAsync(SelectedArtist.Id);
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
            album.AlbumArtUrl = album.GetPrimaryImageUrl(_jellyfinService.ServerUrl, _jellyfinService.CurrentAccessToken);
            album.AlbumSubtitle = SelectedArtist == null ? album.AlbumArtist : album.ProductionYear.ToString();
            Albums.Add(album);
        }

        AlbumCount = $"{Albums.Count} ALBUMS";
    }

    private async Task LoadTracksAsync()
    {
        List<Track>? tracks = [];

        Tracks.Clear();

        if (SelectedArtist != null && SelectedAlbum == null)
        {
            tracks = await _jellyfinService.GetTracksByArtistAsync(SelectedArtist.Id);
        }
        else if (SelectedAlbum != null)
        {
            tracks = await _jellyfinService.GetTracksByAlbumAsync(SelectedAlbum.Id);
        }
        else
        {
            tracks = await _jellyfinService.GetTracksAsync(_selectedLibrary.Id);
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

        var sessions = await _jellyfinService.GetSessionsAsync();
        if (sessions.Count > 0)
        {
            var session = sessions[0];
            await _jellyfinService.PlayTrackAsync(session.Id, SelectedTrack.Id);
        }
    }
}