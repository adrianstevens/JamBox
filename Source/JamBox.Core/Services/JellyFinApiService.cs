using JamBox.Core.Models;
using JamBox.Core.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace JamBox.Core.Services;

public class JellyfinApiService : IJellyfinApiService
{
    private HttpClient? _httpClient;
    private string? _accessToken;
    private string? _userId;

    private const string ClientName = "JamBoxAvalonia";
    private const string ClientVersion = "0.2.0";
    private const string DeviceName = "Desktop";
    private const string DeviceId = "jambox-avalonia-client-guid";

    public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);

    public string? CurrentUserId => _userId;

    public string? CurrentAccessToken => _accessToken;

    public string? ServerUrl => _httpClient?.BaseAddress?.ToString();

    public JellyfinApiService()
    { }

    private void CreateHttpClient(string baseUrl)
    {
        _httpClient?.Dispose();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        var authHeader =
            $"MediaBrowser Client=\"{ClientName}\", Device=\"{DeviceName}\", DeviceId=\"{DeviceId}\", Version=\"{ClientVersion}\"";
        _httpClient.DefaultRequestHeaders.Add("X-Emby-Authorization", authHeader);
    }

    /// <summary>
    /// Sets the base URL for the Jellyfin server.
    /// </summary>
    public void SetServerUrl(string url)
    {
        // The core fix: Set the BaseAddress on the HttpClient
        // This is the source of the "invalid URI" error.
        CreateHttpClient(url.TrimEnd('/') + "/");
    }

    public async Task<PublicSystemInfo?> GetPublicSystemInfoAsync()
    {
        if (_httpClient?.BaseAddress == null)
        {
            return null;
        }

        try
        {
            // Now you can use a relative URI here
            var response = await _httpClient.GetAsync("System/Info/Public");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return null;
            }

            return JsonSerializer.Deserialize(jsonString, AppJsonSerializerContext.Default.PublicSystemInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting public system info: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Authenticates a user with the Jellyfin server.
    /// </summary>
    public async Task<bool> AuthenticateUserAsync(string username, string password)
    {
        if (_httpClient?.BaseAddress == null)
        {
            return false;
        }

        try
        {
            var authPayload = new AuthPayload
            {
                Username = username,
                Pw = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(authPayload, AppJsonSerializerContext.Default.AuthPayload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("Users/AuthenticateByName", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Authentication failed: {response.StatusCode} - {errorContent}");
                return false;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize(jsonString, AppJsonSerializerContext.Default.AuthenticationResult);

            _accessToken = authResult!.AccessToken;
            _userId = authResult.User.Id;

            // Add token header for subsequent requests
            if (_httpClient.DefaultRequestHeaders.Contains("X-Emby-Token"))
            {
                _httpClient.DefaultRequestHeaders.Remove("X-Emby-Token");
            }

            _httpClient.DefaultRequestHeaders.Add("X-Emby-Token", _accessToken);

            Console.WriteLine($"Authenticated successfully. User: {authResult.User.Name}, Token: {_accessToken.Substring(0, 8)}...");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Fetches the current user's media libraries (views).
    /// </summary>
    public async Task<List<MediaCollectionItem>> GetUserMediaViewsAsync()
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(_userId))
        {
            Console.WriteLine("Not authenticated. Please authenticate first.");
            return [];
        }

        try
        {
            // Simplified URI
            var response = await _httpClient!.GetAsync($"Users/{_userId}/Views");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            var userViewsResult = JsonSerializer.Deserialize(jsonString, AppJsonSerializerContext.Default.UserViewsResult);

            return userViewsResult?.Items ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user views: {ex.Message}");
            return [];
        }
    }

    public void Logout()
    {
        _accessToken = null;
        _userId = string.Empty;

        if (_httpClient is not null && _httpClient.DefaultRequestHeaders.Contains("X-Emby-Token"))
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Emby-Token");
        }

        Console.WriteLine("Logged out");
    }

    public async Task<List<Artist>> GetArtistsAsync(string libraryId)
    {
        var response = await _httpClient!.GetAsync($"Items?IncludeItemTypes=MusicArtist&ParentId={libraryId}&Recursive=true");
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync(stream, AppJsonSerializerContext.Default.JellyfinResponseArtist);
        return result?.Items ?? [];
    }

    public async Task<List<Album>> GetAlbumsAsync(string libraryId)
    {
        var response = await _httpClient!.GetAsync($"Items?IncludeItemTypes=MusicAlbum&ParentId={libraryId}&Recursive=true");
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync(stream, AppJsonSerializerContext.Default.JellyfinResponseAlbum);
        return result?.Items ?? [];
    }

    public async Task<List<Album>> GetAlbumsByArtistAsync(string artistId)
    {
        var response = await _httpClient!.GetAsync($"Items?IncludeItemTypes=MusicAlbum&ParentId={artistId}");
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync(stream, AppJsonSerializerContext.Default.JellyfinResponseAlbum);
        return result?.Items ?? [];
    }

    public async Task<List<Track>> GetTracksAsync(string libraryId)
    {
        var response = await _httpClient!.GetAsync($"Items?IncludeItemTypes=Audio&ParentId={libraryId}&Recursive=true");
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync(stream, AppJsonSerializerContext.Default.JellyfinResponseTrack);
        return result?.Items ?? [];
    }

    public async Task<List<Track>> GetTracksByAlbumAsync(string albumId)
    {
        var response = await _httpClient!.GetAsync($"Items?IncludeItemTypes=Audio&ParentId={albumId}&SortBy=IndexNumber");
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync(stream, AppJsonSerializerContext.Default.JellyfinResponseTrack);
        return result?.Items ?? [];
    }

    public async Task<List<Track>> GetTracksByArtistAsync(string artistId)
    {
        var response = await _httpClient!.GetAsync($"Items?IncludeItemTypes=Audio&ArtistIds={artistId}&Recursive=true");
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync(stream, AppJsonSerializerContext.Default.JellyfinResponseTrack);
        return result?.Items ?? [];
    }

    public async Task<List<SessionInfo>> GetSessionsAsync()
    {
        if (_httpClient is null) { return []; }

        var response = await _httpClient.GetAsync("Sessions");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        var sessions = await JsonSerializer.DeserializeAsync(stream, AppJsonSerializerContext.Default.ListSessionInfo);

        return sessions ?? [];
    }

    public async Task PlayTrackAsync(string sessionId, string trackId)
    {
        var payload = new PlaybackPayload
        {
            ItemIds = [trackId],
            PlayCommand = "PlayNow"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, AppJsonSerializerContext.Default.PlaybackPayload),
            Encoding.UTF8, "application/json");

        var response = await _httpClient!.PostAsync($"Sessions/{sessionId}/Playing", content);
        response.EnsureSuccessStatusCode();
    }

    public string GetTrackStreamUrl(string trackId, string container = "mp3")
    {
        if (_httpClient is null || _httpClient.BaseAddress is null)
        {
            return string.Empty;
        }

        return $"{_httpClient.BaseAddress.AbsoluteUri}Audio/{trackId}/universal?UserId={_userId}&DeviceId={DeviceId}&Container={container}&api_key={_accessToken}";
    }

    public async Task<List<Track>> SearchTracksAsync(string query)
    {
        var response = await _httpClient!.GetAsync($"Items?IncludeItemTypes=Audio&Recursive=true&SearchTerm={Uri.EscapeDataString(query)}");
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync(stream, AppJsonSerializerContext.Default.JellyfinResponseTrack);
        return result?.Items ?? [];
    }
}