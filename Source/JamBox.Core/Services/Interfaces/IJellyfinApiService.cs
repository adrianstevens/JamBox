using JamBox.Core.Models;

namespace JamBox.Core.Services.Interfaces;

public interface IJellyfinApiService
{
    bool IsAuthenticated { get; }

    string? CurrentUserId { get; }

    string? CurrentAccessToken { get; }

    string? ServerUrl { get; }

    void SetServerUrl(string url);

    Task<PublicSystemInfo?> GetPublicSystemInfoAsync();

    Task<bool> AuthenticateUserAsync(string username, string password);

    Task<List<BaseItemDto>> GetUserMediaViewsAsync();

    void Logout();

    Task<List<Artist>> GetArtistsAsync(string libraryId);

    Task<List<Album>> GetAlbumsAsync(string libraryId);

    Task<List<Album>> GetAlbumsByArtistAsync(string artistId);

    Task<List<Track>> GetTracksAsync(string libraryId);

    Task<List<Track>> GetTracksByAlbumAsync(string albumId);

    Task<List<Track>> GetTracksByArtistAsync(string artistId);

    Task<List<SessionInfo>> GetSessionsAsync();

    Task PlayTrackAsync(string sessionId, string trackId);

    string GetTrackStreamUrl(string trackId, string container = "mp3");

    Task<List<Track>> SearchTracksAsync(string query);
}
