using System.Text.Json.Serialization;

namespace JamBox.Core.Models;

public class AuthPayload
{
    [JsonPropertyName("Username")]
    public string Username { get; set; }

    [JsonPropertyName("Pw")]
    public string Pw { get; set; }
}