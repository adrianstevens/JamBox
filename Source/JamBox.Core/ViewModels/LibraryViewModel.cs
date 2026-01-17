using JamBox.Core.Models;
using JamBox.Core.Services.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace JamBox.Core.ViewModels;

public class LibraryViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IJellyfinApiService _jellyfinApiService;

    private MediaCollectionItem? _selectedLibrary;

    /// <summary>
    /// The PlaybackViewModel handles all playback-related state and commands.
    /// </summary>
    public PlaybackViewModel Playback { get; }

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

    public ObservableCollection<Album> Albums { get; private set; } = [];

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

    public ObservableCollection<Track> Tracks { get; private set; } = [];

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

    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Album, Unit> PlayAlbumCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadArtistsCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadAlbumsCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadTracksCommand { get; }
    public ReactiveCommand<Unit, Unit> SortArtistsCommand { get; }
    public ReactiveCommand<Unit, Unit> SortAlbumsCommand { get; }
    public ReactiveCommand<Unit, Unit> SortTracksCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetArtistsSelectionCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetAlbumSelectionCommand { get; }
    public ReactiveCommand<Unit, Unit> JukeBoxModeCommand { get; }
    public ReactiveCommand<Unit, Unit> MiniPlayerCommand { get; }

    public LibraryViewModel(
        PlaybackViewModel playbackViewModel,
        INavigationService navigationService,
        IJellyfinApiService jellyfinApiService)
    {
        Playback = playbackViewModel;
        _navigationService = navigationService;
        _jellyfinApiService = jellyfinApiService;

        LoadArtistsCommand = ReactiveCommand.CreateFromTask(LoadArtistsAsync);
        LoadAlbumsCommand = ReactiveCommand.CreateFromTask(LoadAlbumsAsync);
        LoadTracksCommand = ReactiveCommand.CreateFromTask(LoadTracksAsync);
        SortArtistsCommand = ReactiveCommand.CreateFromTask(SortArtistsAsync);
        SortAlbumsCommand = ReactiveCommand.CreateFromTask(SortAlbumsAsync);
        SortTracksCommand = ReactiveCommand.CreateFromTask(SortTracksAsync);

        ResetArtistsSelectionCommand = ReactiveCommand.CreateFromTask(ResetArtistsSelectionAsync);
        ResetAlbumSelectionCommand = ReactiveCommand.CreateFromTask(ResetAlbumSelectionAsync);

        var canPlay = this.WhenAnyValue(vm => vm.SelectedTrack).Select(t => t != null);
        PlayCommand = ReactiveCommand.CreateFromTask(PlaySelectedTrackAsync, canPlay);
        PlayAlbumCommand = ReactiveCommand.CreateFromTask<Album>(PlayAlbumAsync);
        JukeBoxModeCommand = ReactiveCommand.Create(() => _navigationService.NavigateTo<JukeBoxPage, JukeBoxViewModel>());
        MiniPlayerCommand = ReactiveCommand.Create(() => _navigationService.ToggleMiniPlayer());

        // Start library loading with proper error handling
        _ = InitializeLibraryAsync();
    }

    private async Task InitializeLibraryAsync()
    {
        try
        {
            IsBusy = true;
            await LoadLibraryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading library: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
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

        // Prepare all album data first (URLs, subtitles) before updating collection
        foreach (var album in albums)
        {
            album.AlbumArtUrl = album.GetPrimaryImageUrl(_jellyfinApiService.ServerUrl ?? "", _jellyfinApiService.CurrentAccessToken ?? "") ?? "";
            album.AlbumSubtitle = SelectedArtist == null ? album.AlbumArtist : album.ProductionYear.ToString();
        }

        // Batch update: replace entire collection at once to avoid multiple UI updates
        Albums = new ObservableCollection<Album>(albums);
        this.RaisePropertyChanged(nameof(Albums));

        AlbumCount = $"{Albums.Count} ALBUMS";
    }

    private async Task LoadTracksAsync()
    {
        if (_selectedLibrary is null) { return; }

        List<Track>? tracks = [];

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

        // Batch update: replace entire collection at once to avoid multiple UI updates
        Tracks = new ObservableCollection<Track>(tracks);
        this.RaisePropertyChanged(nameof(Tracks));

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

    private Task PlaySelectedTrackAsync()
    {
        if (SelectedTrack == null) { return Task.CompletedTask; }

        // Determine album art URL
        string? albumArtUrl;
        if (SelectedAlbum == null)
        {
            var album = Albums.FirstOrDefault(a => a.Id == SelectedTrack.AlbumId);
            albumArtUrl = album?.AlbumArtUrl;
        }
        else
        {
            albumArtUrl = SelectedAlbum?.AlbumArtUrl;
        }

        // Delegate to PlaybackViewModel
        Playback.SetPlaylist(Tracks, SelectedTrack, albumArtUrl);
        Playback.PlayCommand.Execute().Subscribe();
        return Task.CompletedTask;
    }

    private async Task PlayAlbumAsync(Album album)
    {
        if (album == null) { return; }

        // Load tracks for the album
        var tracks = await _jellyfinApiService.GetTracksByAlbumAsync(album.Id);
        if (tracks == null || tracks.Count == 0) { return; }

        // Sort by track number
        tracks = tracks.OrderBy(t => t.IndexNumber).ToList();

        // Update the tracks collection
        Tracks = new ObservableCollection<Track>(tracks);
        this.RaisePropertyChanged(nameof(Tracks));
        TrackCount = $"{Tracks.Count} TRACKS";

        // Select the first track and start playback
        SelectedTrack = Tracks.FirstOrDefault();
        if (SelectedTrack != null)
        {
            Playback.SetPlaylist(Tracks, SelectedTrack, album.AlbumArtUrl);
            Playback.PlayCommand.Execute().Subscribe();
        }
    }
}