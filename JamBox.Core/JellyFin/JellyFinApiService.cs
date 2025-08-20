using JamBox.Jellyfin;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JamBox.JellyFin
{
    public class JellyfinApiService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private string _userId;
        private string _jellyfinServerUrl;

        private const string ClientName = "JamBoxAvalonia";
        private const string ClientVersion = "0.1.0";
        private const string DeviceName = "Desktop";
        private const string DeviceId = "jambox-avalonia-client-guid";

        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);
        public string CurrentUserId => _userId;

        public JellyfinApiService()
        {
            _httpClient = new HttpClient();
            // Optional: Configure default request headers if needed globally
            _httpClient.DefaultRequestHeaders.Add("X-Emby-Client", ClientName);
            _httpClient.DefaultRequestHeaders.Add("X-Emby-Device-Name", DeviceName);
            _httpClient.DefaultRequestHeaders.Add("X-Emby-Device-Id", DeviceId);
            _httpClient.DefaultRequestHeaders.Add("X-Emby-Client-Version", ClientVersion);
        }

        /// <summary>
        /// Sets the base URL for the Jellyfin server.
        /// </summary>
        /// <param name="url">The base URL of the Jellyfin server (e.g., "http://192.168.68.100:8096").</param>
        public void SetServerUrl(string url)
        {
            _jellyfinServerUrl = url.TrimEnd('/');
        }

        /// <summary>
        /// Gets public information about the Jellyfin server.
        /// Does not require authentication.
        /// </summary>
        /// <returns>A PublicSystemInfo object if successful, null otherwise.</returns>
        public async Task<PublicSystemInfo> GetPublicSystemInfoAsync()
        {
            if (string.IsNullOrEmpty(_jellyfinServerUrl))
            {
                Console.WriteLine("Server URL is not set.");
                return null;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_jellyfinServerUrl}/System/Info/Public");
                response.EnsureSuccessStatusCode(); // Throws if not a success status code
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PublicSystemInfo>(jsonString);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error getting public system info: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Authenticates a user with the Jellyfin server.
        /// </summary>
        /// <param name="username">The Jellyfin username.</param>
        /// <param name="password">The Jellyfin password.</param>
        /// <returns>True if authentication is successful, false otherwise.</returns>
        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(_jellyfinServerUrl))
            {
                Console.WriteLine("Server URL is not set. Cannot authenticate.");
                return false;
            }

            try
            {
                var authPayload = new
                {
                    Username = username,
                    Pw = password // Jellyfin API expects 'Pw' for password in this endpoint
                };
                var jsonPayload = JsonSerializer.Serialize(authPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_jellyfinServerUrl}/Users/AuthenticateByName", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var authResult = JsonSerializer.Deserialize<AuthenticationResult>(jsonString);

                    _accessToken = authResult.AccessToken;
                    _userId = authResult.User.Id;

                    // Set Authorization header for subsequent authenticated requests
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("MediaBrowser",
                            $"Token=\"{_accessToken}\", Client=\"{ClientName}\", Device=\"{DeviceName}\", DeviceId=\"{DeviceId}\", Version=\"{ClientVersion}\"");

                    Console.WriteLine($"Authenticated successfully. User: {authResult.User.Name}, Access Token: {_accessToken.Substring(0, 8)}...");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Authentication failed: {response.StatusCode} - {errorContent}");
                    _accessToken = null;
                    _userId = null;
                    _httpClient.DefaultRequestHeaders.Authorization = null; // Clear token on failure
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error during authentication: {ex.Message}");
                return false;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON deserialization error during authentication: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Fetches the current user's media libraries (views).
        /// Requires prior authentication.
        /// </summary>
        /// <returns>A list of BaseItemDto representing the user's libraries, or null if not authenticated or an error occurs.</returns>
        public async Task<List<BaseItemDto>> GetUserMediaViewsAsync()
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(_userId))
            {
                Console.WriteLine("Not authenticated. Please authenticate first to get user views.");
                return null;
            }
            if (string.IsNullOrEmpty(_jellyfinServerUrl))
            {
                Console.WriteLine("Server URL is not set. Cannot get user views.");
                return null;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_jellyfinServerUrl}/Users/{_userId}/Views");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var userViewsResult = JsonSerializer.Deserialize<UserViewsResult>(jsonString);
                return userViewsResult?.Items;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error getting user media views: {ex.Message}");
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON deserialization error getting user media views: {ex.Message}");
                return null;
            }
        }

        // You might want a logout method to clear the token
        public void Logout()
        {
            _accessToken = null;
            _userId = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Console.WriteLine("Logged out.");
        }
    }
}
