using System.Text.Json.Serialization;

namespace JamBox.Core.Models;

public class AuthenticationResult
{
    [JsonPropertyName("User")]
    public required User User { get; set; }

    [JsonPropertyName("AccessToken")]
    public required string AccessToken { get; set; }
}

public class User
{
    [JsonPropertyName("Id")]
    public required string Id { get; set; }

    [JsonPropertyName("Name")]
    public required string Name { get; set; }
}

public class UserData
{
    [JsonPropertyName("PlaybackPositionTicks")]
    public long PlaybackPositionTicks { get; set; }

    [JsonPropertyName("PlayCount")]
    public int PlayCount { get; set; }

    [JsonPropertyName("IsFavorite")]
    public bool IsFavorite { get; set; }

    [JsonPropertyName("Played")]
    public bool Played { get; set; }

    [JsonPropertyName("Key")]
    public string Key { get; set; }

    [JsonPropertyName("ItemId")]
    public string ItemId { get; set; }
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

public class MediaCollectionItem
{
    [JsonPropertyName("Id")]
    public string Id { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("CollectionType")]
    public string CollectionType { get; set; } // e.g., "movies", "music", "tvshows"

    public string DisplayName => $"{Name} ({CollectionType})";
}

public class UserViewsResult
{
    [JsonPropertyName("Items")]
    public List<MediaCollectionItem> Items { get; set; }
}

public class Artist
{
    [JsonPropertyName("Id")]
    public string Id { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("ImageTags")]
    public Dictionary<string, string> ImageTags { get; set; }

    [JsonPropertyName("Genres")]
    public List<string> Genres { get; set; }

    [JsonPropertyName("ProductionYear")]
    public int? ProductionYear { get; set; }

    public string? PrimaryImageUrl { get; private set; }

    public string? GetPrimaryImageUrl(string serverUrl, string accessToken, int width = 300, int height = 300)
    {
        if (ImageTags != null && ImageTags.TryGetValue("Primary", out var tag))
        {
            PrimaryImageUrl = $"{serverUrl}/Items/{Id}/Images/Primary?tag={tag}&quality=90&fillWidth={width}&fillHeight={height}&cropWhitespace=true&api_key={accessToken}";
            return PrimaryImageUrl;
        }
        return null;
    }
}

public class AlbumArtistInfo
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    // Add other properties if needed
}

public class Album
{
    [JsonPropertyName("Id")]
    public string Id { get; set; }

    [JsonPropertyName("Name")]
    public string Title { get; set; }

    [JsonPropertyName("ProductionYear")]
    public int ProductionYear { get; set; }

    [JsonPropertyName("AlbumArtists")]
    public List<AlbumArtistInfo> AlbumArtists { get; set; }

    [JsonPropertyName("UserData")]
    public UserData UserData { get; set; }

    public string AlbumArtistsString => AlbumArtists == null ? "" : string.Join(", ", AlbumArtists);

    [JsonPropertyName("ImageTags")]
    public Dictionary<string, string> ImageTags { get; set; }

    [JsonIgnore]
    public string AlbumSubtitle { get; set; }

    [JsonIgnore]
    public string AlbumArtist => AlbumArtists?.FirstOrDefault().Name ?? "Unknown Artist";

    [JsonIgnore]
    public string AlbumArtUrl { get; set; }

    /// <summary>
    /// Returns the URL for the primary album cover.
    /// </summary>
    public string? GetPrimaryImageUrl(string serverUrl, string accessToken, int width = 300, int height = 300)
    {
        if (!string.IsNullOrEmpty(Id) && ImageTags != null && ImageTags.TryGetValue("Primary", out var tag))
        {
            var baseUrl = serverUrl.TrimEnd('/');
            var url = $"{baseUrl}/Items/{Id}/Images/Primary" +
                      $"?tag={tag}&quality=90&fillWidth={width}&fillHeight={height}" +
                      $"&cropWhitespace=true&api_key={accessToken}";
            return url;
        }

        return null;
    }
}

public class ArtistInfo
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Id")]
    public string Id { get; set; }
}

public class Track
{
    [JsonPropertyName("Id")]
    public string Id { get; set; }

    [JsonPropertyName("Name")]
    public string Title { get; set; }

    [JsonPropertyName("AlbumId")]
    public string AlbumId { get; set; }

    [JsonPropertyName("IndexNumber")]
    public int IndexNumber { get; set; }

    [JsonPropertyName("RunTimeTicks")]
    public long RunTimeTicks { get; set; }

    public TimeSpan Duration => TimeSpan.FromTicks(RunTimeTicks);
}

public class ItemResult<T>
{
    [JsonPropertyName("Items")]
    public List<T> Items { get; set; }

    [JsonPropertyName("TotalRecordCount")]
    public int TotalRecordCount { get; set; }
}

public class JellyfinResponse<T>
{
    public List<T> Items { get; set; }
    public int TotalRecordCount { get; set; }
}

public class SessionInfo
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Client { get; set; }       // e.g., "Jellyfin Web", "Android TV"
    public string DeviceName { get; set; }   // e.g., "Living Room TV"
    public string NowPlayingItemId { get; set; }

    public string RemoteEndPoint { get; set; }
    public bool SupportsRemoteControl { get; set; }
}