using JamBox.Core.Models;
using JamBox.Core.Services.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace JamBox.Core.Services;

public class JellyfinApiService : IJellyfinApiService
{
    private HttpClient _httpClient;
    private string? _accessToken;
    private string _userId;

    private const string ClientName = "JamBoxAvalonia";
    private const string ClientVersion = "0.1.0";
    private const string DeviceName = "Desktop";
    private const string DeviceId = "jambox-avalonia-client-guid";

    public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);
    public string CurrentUserId => _userId;

    public string? CurrentAccessToken => _accessToken;

    public string? ServerUrl => _httpClient.BaseAddress?.ToString();

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
        if (_httpClient.BaseAddress == null)
            return null;

        try
        {
            // Now you can use a relative URI here
            var response = await _httpClient.GetAsync("System/Info/Public");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PublicSystemInfo>(jsonString);
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
        if (_httpClient.BaseAddress == null)
            return false;

        try
        {
            var authPayload = new
            {
                Username = username,
                Pw = password
            };

            var content = new StringContent(JsonSerializer.Serialize(authPayload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Users/AuthenticateByName", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Authentication failed: {response.StatusCode} - {errorContent}");
                return false;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<AuthenticationResult>(jsonString);

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
    public async Task<List<BaseItemDto>> GetUserMediaViewsAsync()
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(_userId))
        {
            Console.WriteLine("Not authenticated. Please authenticate first.");
            return [];
        }

        try
        {
            // Simplified URI
            var response = await _httpClient.GetAsync($"Users/{_userId}/Views");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            var userViewsResult = JsonSerializer.Deserialize<UserViewsResult>(jsonString);
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

        if (_httpClient.DefaultRequestHeaders.Contains("X-Emby-Token"))
            _httpClient.DefaultRequestHeaders.Remove("X-Emby-Token");

        Console.WriteLine("Logged out.");
    }

    public async Task<List<Artist>> GetArtistsAsync(string libraryId)
    {
        var response = await _httpClient.GetFromJsonAsync<JellyfinResponse<Artist>>(
            $"Items?IncludeItemTypes=MusicArtist&ParentId={libraryId}&Recursive=true"
        );
        return response?.Items ?? [];
    }

    public async Task<List<Album>> GetAlbumsAsync(string libraryId)
    {
        var response = await _httpClient.GetFromJsonAsync<JellyfinResponse<Album>>(
            $"Items?IncludeItemTypes=MusicAlbum&ParentId={libraryId}&Recursive=true"
        );
        return response?.Items ?? [];
    }

    public async Task<List<Album>> GetAlbumsByArtistAsync(string artistId)
    {
        var response = await _httpClient.GetFromJsonAsync<JellyfinResponse<Album>>(
            $"Items?IncludeItemTypes=MusicAlbum&ParentId={artistId}"
        );
        return response?.Items ?? [];
    }

    public async Task<List<Track>> GetTracksAsync(string libraryId)
    {
        var response = await _httpClient.GetFromJsonAsync<JellyfinResponse<Track>>(
            $"Items?IncludeItemTypes=Audio&ParentId={libraryId}&Recursive=true"
        );
        return response?.Items ?? [];
    }

    public async Task<List<Track>> GetTracksByAlbumAsync(string albumId)
    {
        var response = await _httpClient.GetFromJsonAsync<JellyfinResponse<Track>>(
            $"Items?IncludeItemTypes=Audio&ParentId={albumId}&SortBy=IndexNumber"
        );
        return response?.Items ?? [];
    }

    public async Task<List<Track>> GetTracksByArtistAsync(string artistId)
    {
        var response = await _httpClient.GetFromJsonAsync<JellyfinResponse<Track>>(
            $"Items?IncludeItemTypes=Audio&ArtistIds={artistId}&Recursive=true"
        );
        return response?.Items ?? [];
    }

    public async Task<List<SessionInfo>> GetSessionsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<SessionInfo>>("Sessions");
        return response ?? [];
    }

    public async Task PlayTrackAsync(string sessionId, string trackId)
    {
        var payload = new
        {
            ItemIds = new[] { trackId },
            PlayCommand = "PlayNow"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"Sessions/{sessionId}/Playing", content);
        response.EnsureSuccessStatusCode();
    }

    public string GetTrackStreamUrl(string trackId, string container = "mp3")
    {
        return $"{_httpClient.BaseAddress.AbsoluteUri}Audio/{trackId}/universal?UserId={_userId}&DeviceId={DeviceId}&Container={container}&api_key={_accessToken}";
    }

    public async Task<List<Track>> SearchTracksAsync(string query)
    {
        var response = await _httpClient.GetFromJsonAsync<JellyfinResponse<Track>>(
            $"Items?IncludeItemTypes=Audio&Recursive=true&SearchTerm={Uri.EscapeDataString(query)}"
        );
        return response?.Items ?? [];
    }
}