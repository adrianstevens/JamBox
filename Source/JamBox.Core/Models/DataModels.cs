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
    public string Key { get; set; } = default!;

    [JsonPropertyName("ItemId")]
    public string ItemId { get; set; } = default!;
}

public class PublicSystemInfo
{
    [JsonPropertyName("ServerName")]
    public string ServerName { get; set; } = default!;

    [JsonPropertyName("Version")]
    public string Version { get; set; } = default!;

    [JsonPropertyName("Id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("DeviceName")]
    public string DeviceName { get; set; } = default!;
}

public class MediaCollectionItem
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("CollectionType")]
    public string CollectionType { get; set; } = default!; // e.g., "movies", "music", "tvshows"

    public string DisplayName => $"{Name} ({CollectionType})";
}

public class UserViewsResult
{
    [JsonPropertyName("Items")]
    public List<MediaCollectionItem> Items { get; set; } = default!;
}

public class Artist
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("ImageTags")]
    public Dictionary<string, string> ImageTags { get; set; } = default!;

    [JsonPropertyName("Genres")]
    public List<string> Genres { get; set; } = default!;

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
    public string Name { get; set; } = default!;
    // Add other properties if needed
}

public class Album
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("Name")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("ProductionYear")]
    public int ProductionYear { get; set; }

    [JsonPropertyName("AlbumArtists")]
    public List<AlbumArtistInfo> AlbumArtists { get; set; } = default!;

    [JsonPropertyName("UserData")]
    public UserData UserData { get; set; } = default!;

    public string AlbumArtistsString => AlbumArtists == null ? "" : string.Join(", ", AlbumArtists);

    [JsonPropertyName("ImageTags")]
    public Dictionary<string, string> ImageTags { get; set; } = default!;

    [JsonIgnore]
    public string AlbumSubtitle { get; set; } = default!;

    [JsonIgnore]
    public string AlbumArtist => AlbumArtists?.FirstOrDefault()?.Name ?? "Unknown Artist";

    [JsonIgnore]
    public string AlbumArtUrl { get; set; } = default!;

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
    public string Name { get; set; } = default!;

    [JsonPropertyName("Id")]
    public string Id { get; set; } = default!;
}

public class Track
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("Name")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("AlbumId")]
    public string AlbumId { get; set; } = default!;

    [JsonPropertyName("AlbumArtist")]
    public string? AlbumArtist { get; set; }

    [JsonPropertyName("IndexNumber")]
    public int IndexNumber { get; set; } = default!;

    [JsonPropertyName("RunTimeTicks")]
    public long RunTimeTicks { get; set; }

    [JsonIgnore]
    public bool IsPlaying { get; set; }

    public TimeSpan Duration => TimeSpan.FromTicks(RunTimeTicks);
}

public class JellyfinResponse<T>
{
    [JsonPropertyName("Items")]
    public List<T> Items { get; set; } = default!;

    [JsonPropertyName("TotalRecordCount")]
    public int TotalRecordCount { get; set; }
}

public class SessionInfo
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Client { get; set; } = default!;      // e.g., "Jellyfin Web", "Android TV"
    public string DeviceName { get; set; } = default!;   // e.g., "Living Room TV"
    public string NowPlayingItemId { get; set; } = default!;

    public string RemoteEndPoint { get; set; } = default!;
    public bool SupportsRemoteControl { get; set; }
}

/// <summary>
/// Model for deserializing the response from the /Items endpoint when only the count is requested (Limit=0).
/// This structure efficiently extracts the TotalRecordCount.
/// </summary>
public class JellyfinCountResponse
{
    [JsonPropertyName("Items")]
    public System.Text.Json.JsonElement Items { get; set; }

    [JsonPropertyName("TotalRecordCount")]
    public int TotalRecordCount { get; set; }

    [JsonPropertyName("StartIndex")]
    public int StartIndex { get; set; }
}