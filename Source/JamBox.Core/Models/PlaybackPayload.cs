using System.Text.Json.Serialization;

namespace JamBox.Core.Models;

public class PlaybackPayload
{
    [JsonPropertyName("ItemIds")]
    public string[] ItemIds { get; set; }

    [JsonPropertyName("PlayCommand")]
    public string PlayCommand { get; set; }
}