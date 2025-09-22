using JamBox.Core.Models;
using JamBox.Core.Settings;
using System.Text.Json.Serialization;

namespace JamBox.Core;

[JsonSerializable(typeof(AuthenticationResult))]
[JsonSerializable(typeof(AuthPayload))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(UserData))]
[JsonSerializable(typeof(PublicSystemInfo))]
[JsonSerializable(typeof(MediaCollectionItem))]
[JsonSerializable(typeof(UserViewsResult))]
[JsonSerializable(typeof(Artist))]
[JsonSerializable(typeof(AlbumArtistInfo))]
[JsonSerializable(typeof(Album))]
[JsonSerializable(typeof(ArtistInfo))]
[JsonSerializable(typeof(Track))]
[JsonSerializable(typeof(ItemResult<MediaCollectionItem>))]
[JsonSerializable(typeof(ItemResult<Artist>))]
[JsonSerializable(typeof(ItemResult<Album>))]
[JsonSerializable(typeof(ItemResult<Track>))]
[JsonSerializable(typeof(JellyfinResponse<MediaCollectionItem>))]
[JsonSerializable(typeof(JellyfinResponse<Artist>))]
[JsonSerializable(typeof(JellyfinResponse<Album>))]
[JsonSerializable(typeof(JellyfinResponse<Track>))]
[JsonSerializable(typeof(SessionInfo))]
[JsonSerializable(typeof(AuthPayload))]
[JsonSerializable(typeof(PlaybackPayload))]
[JsonSerializable(typeof(UserCredentials))]
[JsonSerializable(typeof(WindowSettings))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}