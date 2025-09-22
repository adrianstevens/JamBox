using System.Text.Json.Serialization;

namespace JamBox.Core.Services;

[JsonSerializable(typeof(JellyfinApiService))]
public partial class JellyfinApiServiceContext : JsonSerializerContext
{ }