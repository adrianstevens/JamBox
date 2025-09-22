using System.Text.Json.Serialization;

namespace JamBox.Core.Models;

public class PlaybackPayload
{
    [JsonPropertyName("ItemIds")]
    public required string[] ItemIds { get; set; }

    [JsonPropertyName("PlayCommand")]
    public required string PlayCommand { get; set; }
}