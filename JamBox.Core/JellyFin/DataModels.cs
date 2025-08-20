using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JamBox.Jellyfin
{
    public class AuthenticationResult
    {
        [JsonPropertyName("User")]
        public User User { get; set; }

        [JsonPropertyName("AccessToken")]
        public string AccessToken { get; set; }
    }

    public class User
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

    }

    public class PublicSystemInfo
    {
        [JsonPropertyName("ServerName")]
        public string ServerName { get; set; }

        [JsonPropertyName("Version")]
        public string Version { get; set; }

        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("DeviceName")]
        public string DeviceName { get; set; }

    }

    public class BaseItemDto
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("CollectionType")]
        public string CollectionType { get; set; } // e.g., "movies", "music", "tvshows"

        // Add other properties relevant to media items (e.g., ImageTags, Type)
    }

    public class UserViewsResult
    {
        [JsonPropertyName("Items")]
        public List<BaseItemDto> Items { get; set; }
    }


}