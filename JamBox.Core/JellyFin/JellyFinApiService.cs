using System.Text;
using System.Text.Json;

namespace JamBox.Core.JellyFin
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

            var authHeader =
                $"MediaBrowser Client=\"{ClientName}\", Device=\"{DeviceName}\", DeviceId=\"{DeviceId}\", Version=\"{ClientVersion}\"";

            _httpClient.DefaultRequestHeaders.Add("X-Emby-Authorization", authHeader);
        }

        /// <summary>
        /// Sets the base URL for the Jellyfin server.
        /// </summary>
        public void SetServerUrl(string url)
        {
            _jellyfinServerUrl = url.TrimEnd('/');
        }

        public async Task<PublicSystemInfo> GetPublicSystemInfoAsync()
        {
            if (string.IsNullOrEmpty(_jellyfinServerUrl))
                return null;

            try
            {
                var response = await _httpClient.GetAsync($"{_jellyfinServerUrl}/System/Info/Public");
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
            if (string.IsNullOrEmpty(_jellyfinServerUrl))
                return false;

            try
            {
                var authPayload = new
                {
                    Username = username,
                    Pw = password
                };

                var content = new StringContent(JsonSerializer.Serialize(authPayload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_jellyfinServerUrl}/Users/AuthenticateByName", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Authentication failed: {response.StatusCode} - {errorContent}");
                    return false;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<AuthenticationResult>(jsonString);

                _accessToken = authResult.AccessToken;
                _userId = authResult.User.Id;

                // Add token header for subsequent requests
                if (_httpClient.DefaultRequestHeaders.Contains("X-Emby-Token"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Emby-Token");

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
                return null;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_jellyfinServerUrl}/Users/{_userId}/Views");
                var jsonString = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

                var userViewsResult = JsonSerializer.Deserialize<UserViewsResult>(jsonString);
                return userViewsResult?.Items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user views: {ex.Message}");
                return null;
            }
        }

        public void Logout()
        {
            _accessToken = null;
            _userId = null;

            if (_httpClient.DefaultRequestHeaders.Contains("X-Emby-Token"))
                _httpClient.DefaultRequestHeaders.Remove("X-Emby-Token");

            Console.WriteLine("Logged out.");
        }
    }
}
