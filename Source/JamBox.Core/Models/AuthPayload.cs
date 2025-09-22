using System.Text.Json.Serialization;

namespace JamBox.Core.Models;

public class AuthPayload
{
    [JsonPropertyName("Username")]
    public required string Username { get; set; }

    [JsonPropertyName("Pw")]
    public required string Pw { get; set; }
}