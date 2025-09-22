using JamBox.Core.Models;
using System.Text.Json.Serialization;

namespace JamBox.Core;

[JsonSerializable(typeof(AuthenticationResult))]
[JsonSerializable(typeof(AuthPayload))] // You'll need to define this class if you haven't yet
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(UserData))]
[JsonSerializable(typeof(PublicSystemInfo))]
[JsonSerializable(typeof(BaseItemDto))]
[JsonSerializable(typeof(UserViewsResult))]
[JsonSerializable(typeof(Artist))]
[JsonSerializable(typeof(AlbumArtistInfo))]
[JsonSerializable(typeof(Album))]
[JsonSerializable(typeof(ArtistInfo))]
[JsonSerializable(typeof(Track))]
[JsonSerializable(typeof(ItemResult<BaseItemDto>))] // Example of a generic type
[JsonSerializable(typeof(ItemResult<Artist>))]
[JsonSerializable(typeof(ItemResult<Album>))]
[JsonSerializable(typeof(ItemResult<Track>))]
[JsonSerializable(typeof(JellyfinResponse<BaseItemDto>))]
[JsonSerializable(typeof(JellyfinResponse<Artist>))]
[JsonSerializable(typeof(JellyfinResponse<Album>))]
[JsonSerializable(typeof(JellyfinResponse<Track>))]
[JsonSerializable(typeof(SessionInfo))]
[JsonSerializable(typeof(AuthPayload))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}